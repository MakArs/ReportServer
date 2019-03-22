using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Monik.Common;
using ReportService.Extensions;
using ReportService.Interfaces.Core;
using ReportService.Interfaces.ReportTask;
using ReportService.Operations.DataImporters;

namespace ReportService.ReportTask
{
    public class TaskWorker : ITaskWorker
    {
        private readonly IRepository repository;
        private readonly IMapper mapper;
        private readonly IMonik monik;

        public TaskWorker(IRepository repository, IMapper mapper,
            IMonik monik)
        {
            this.repository = repository;
            this.mapper = mapper;
            this.monik = monik;
        }

        private string GetOperationStateFromInstance(string operName, DtoOperInstance instance)
        {
            var state = EnumExtensions.EnumHelper.GetEnumValue<InstanceState>(instance.State);

            return operName +
                   $" (State: {state.ToString()}," +
                   $" started: {instance.StartTime}" +
                   (instance.Duration > 0
                       ? $", duration: {instance.Duration / 60000} m {(instance.Duration % 60000) / 1000.0:f0} s"
                       : "")
                   + ")";
        }

        public void RunOperations(IRTaskRunContext taskContext)
        {
            Stopwatch duration = new Stopwatch();

            duration.Start();

            bool deleteFolder = false;

            if (taskContext.OpersToExecute.Any(oper => oper is SshImporter))//todo:not sshimporter but needsfolder
            {
                deleteFolder = true;
                taskContext.CreateDataFolder();
            }

            taskContext.PackageStates = taskContext.OpersToExecute
                .Select(oper => oper.Properties.Name + " (Not started) ").ToList();

            var dtoTaskInstance = taskContext.TaskInstance;

            var success = true;
            var exceptions = new List<Tuple<Exception, string>>();

            try
            {
                foreach (var oper in taskContext.OpersToExecute)
                {
                    if (taskContext.CancelSource.IsCancellationRequested)
                    {
                        taskContext.CancelSource.Dispose();
                        success = false;
                        break;
                    }

                    var dtoOperInstance = new DtoOperInstance
                    {
                        TaskInstanceId = dtoTaskInstance.Id,
                        OperationId = oper.Properties.Id,
                        StartTime = DateTime.Now,
                        Duration = 0,
                        State = (int) InstanceState.InProcess
                    };

                    dtoOperInstance.Id =
                        repository.CreateEntity(dtoOperInstance);

                    taskContext.PackageStates[oper.Properties.Number - 1] =
                        GetOperationStateFromInstance(oper.Properties.Name, dtoOperInstance);

                    Stopwatch operDuration = new Stopwatch();
                    operDuration.Start();

                    try
                    {
                        Task.Run(async () => await oper
                            .ExecuteAsync(taskContext)).Wait(taskContext.CancelSource.Token);

                        if (oper.Properties.NeedSavePackage)
                            dtoOperInstance.DataSet =
                                taskContext.GetCompressedPackage(oper.Properties.PackageName);

                        dtoOperInstance.State = (int) InstanceState.Success;
                        operDuration.Stop();
                        dtoOperInstance.Duration =
                            Convert.ToInt32(operDuration.ElapsedMilliseconds);
                        repository.UpdateEntity(dtoOperInstance);
                    }

                    catch (Exception e)
                    {
                        if (e is OperationCanceledException)
                            dtoOperInstance.State = (int) InstanceState.Canceled;

                        else
                        {
                            if (e.InnerException == null)
                            {
                                exceptions.Add(new Tuple<Exception, string>(e, oper.Properties.Name));
                                dtoOperInstance.ErrorMessage = e.Message;
                            }

                            else
                            {
                                var allExceptions = e.FromHierarchy(ex => ex.InnerException).ToList();

                                exceptions.AddRange(allExceptions
                                    .Select(exx => new Tuple<Exception, string>(exx, oper.Properties.Name)));

                                dtoOperInstance.ErrorMessage =
                                    string.Join("\n", allExceptions.Select(exx => exx.Message));
                            }

                            dtoOperInstance.State = (int) InstanceState.Failed;
                        }

                        operDuration.Stop();
                        dtoOperInstance.Duration =
                            Convert.ToInt32(operDuration.ElapsedMilliseconds);
                        repository.UpdateEntity(dtoOperInstance);
                    }

                    finally
                    {
                        taskContext.PackageStates[oper.Properties.Number - 1] =
                            GetOperationStateFromInstance(oper.Properties.Name, dtoOperInstance);
                    }
                }

                if (exceptions.Count == 0 || dtoTaskInstance.State == (int) InstanceState.Canceled)
                {
                    var msg = dtoTaskInstance.State == (int) InstanceState.Canceled
                        ? $"Задача {taskContext.TaskId} остановлена"
                        : $"Задача {taskContext.TaskId} успешно выполнена";
                    monik.ApplicationInfo(msg);
                    Console.WriteLine(msg);
                }

                else
                {
                    success = false;
                    taskContext.Exporter.SendError(exceptions, taskContext.TaskName);
                    var msg = $"Задача {taskContext.TaskId} выполнена с ошибками";
                    monik.ApplicationInfo(msg);
                    Console.WriteLine(msg);
                }
            }

            catch (Exception e)
            {
                success = false;
                taskContext.Exporter.SendError(exceptions, taskContext.TaskName);
                var msg = $"Задача {taskContext.TaskId} не выполнена.Возникла ошибка: {e.Message}";
                monik.ApplicationError(msg);
                Console.WriteLine(msg);
            }

            duration.Stop();

            if (deleteFolder)
                taskContext.RemoveDataFolder();

            dtoTaskInstance.Duration = Convert.ToInt32(duration.ElapsedMilliseconds);

            dtoTaskInstance.State =
                success ? (int) InstanceState.Success
                : dtoTaskInstance.State == (int) InstanceState.Canceled ? (int) InstanceState.Canceled
                : (int) InstanceState.Failed;

            repository.UpdateEntity(dtoTaskInstance);
        }

        public async Task<string> RunOperationsAndGetLastView(IRTaskRunContext taskContext)
        {
            await Task.Factory.StartNew(() =>
                RunOperations(taskContext));

            var val = taskContext.Packages.LastOrDefault().Value;

            return taskContext.Exporter.GetDefaultView(taskContext.TaskName, val);
        }

        public async void RunOperationsAndSendLastView(IRTaskRunContext taskContext,
            string mailAddress)
        {
            var view = await RunOperationsAndGetLastView(taskContext);

            if (string.IsNullOrEmpty(view)) return;

            taskContext.Exporter.ForceSend(view, taskContext.TaskName, mailAddress);
        }
    }
}