using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Google.Protobuf;
using Monik.Common;
using ReportService.Interfaces.Core;
using ReportService.Interfaces.ReportTask;

namespace ReportService.ReportTask
{
    public class TaskWorker : ITaskWorker
    {
        private readonly IRepository repository;
        private readonly IMapper mapper;
        private readonly IArchiver archiver;
        private readonly IMonik monik;

        public TaskWorker(IRepository repository, IMapper mapper,
            IMonik monik, IArchiver archiver)
        {
            this.repository = repository;
            this.archiver = archiver;
            this.mapper = mapper;
            this.monik = monik;
        }

        public void RunOperations(IRTaskRunContext taskContext)
        {
            Stopwatch duration = new Stopwatch();

            var dtoTaskInstance = taskContext.TaskInstance;

            duration.Start();

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

                    Stopwatch operDuration = new Stopwatch();
                    operDuration.Start();

                    try
                    {
                        Task.Run(async () => await oper
                            .ExecuteAsync(taskContext)).Wait(taskContext.CancelSource.Token);
                        //importer.Execute(taskContext);

                        using (var stream = new MemoryStream())
                        {
                            taskContext.Packages[oper.Properties.PackageName].WriteTo(stream);
                            dtoOperInstance.DataSet = archiver.CompressStream(stream);
                        }

                        dtoOperInstance.State = (int) InstanceState.Success;
                        operDuration.Stop();
                        dtoOperInstance.Duration =
                            Convert.ToInt32(operDuration.ElapsedMilliseconds);
                        repository.UpdateEntity(dtoOperInstance);
                    }
                    catch (Exception e)
                    {
                        exceptions.Add(new Tuple<Exception, string>(e, oper.Properties.Name));
                        dtoOperInstance.ErrorMessage = e.Message;
                        dtoOperInstance.State = (int) InstanceState.Failed;
                        operDuration.Stop();
                        dtoOperInstance.Duration =
                            Convert.ToInt32(operDuration.ElapsedMilliseconds);
                        repository.UpdateEntity(dtoOperInstance);
                    }

                    //switch (oper)
                    //{
                    //    case IDataImporter importer:
                    //        try
                    //        {
                    //            Task.Run(async () => await importer
                    //                 .ExecuteAsync(taskContext)).Wait(taskContext.CancelSource.Token);
                    //            //importer.Execute(taskContext);

                    //            using (var stream = new MemoryStream())
                    //            {
                    //                taskContext.Packages[importer.PackageName].WriteTo(stream);
                    //                dtoOperInstance.DataSet = archiver.CompressStream(stream);
                    //            }

                    //            dtoOperInstance.State = (int) InstanceState.Success;
                    //            operDuration.Stop();
                    //            dtoOperInstance.Duration =
                    //                Convert.ToInt32(operDuration.ElapsedMilliseconds);
                    //            repository.UpdateEntity(dtoOperInstance);
                    //        }
                    //        catch (Exception e)
                    //        {
                    //            exceptions.Add(new Tuple<Exception, string>(e, importer.Name));
                    //            dtoOperInstance.ErrorMessage = e.Message;
                    //            dtoOperInstance.State = (int) InstanceState.Failed;
                    //            operDuration.Stop();
                    //            dtoOperInstance.Duration =
                    //                Convert.ToInt32(operDuration.ElapsedMilliseconds);
                    //            repository.UpdateEntity(dtoOperInstance);
                    //        }

                    //        break;

                    //    case IDataExporter exporter:

                    //        try
                    //        {
                    //            exporter.Send(taskContext);
                    //            dtoOperInstance.State = (int) InstanceState.Success;
                    //            operDuration.Stop();
                    //            dtoOperInstance.Duration =
                    //                Convert.ToInt32(operDuration.ElapsedMilliseconds);
                    //            repository.UpdateEntity(dtoOperInstance);
                    //        }
                    //catch (Exception e)
                    //{
                    //    exceptions.Add(new Tuple<Exception, string>(e, exporter.Name));
                    //    dtoOperInstance.ErrorMessage = e.Message;
                    //    dtoOperInstance.State = (int) InstanceState.Failed;
                    //    operDuration.Stop();
                    //    dtoOperInstance.Duration =
                    //        Convert.ToInt32(operDuration.ElapsedMilliseconds);
                    //    repository.UpdateEntity(dtoOperInstance);
                    //}

                    //break;
                    //}
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