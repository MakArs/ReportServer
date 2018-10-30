using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Monik.Common;
using ReportService.Interfaces.Core;
using ReportService.Interfaces.ReportTask;

namespace ReportService.ReportTask
{
    public class TaskWorker: ITaskWorker
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

        public void RunOperations(List<IOperation> opers, IRTaskRunContext taskContext)
        {
            Stopwatch duration = new Stopwatch();

            duration.Start();

            var success = true;
            var exceptions = new List<Tuple<Exception, string>>();

            var dtoTaskInstance = new DtoTaskInstance
            {
                TaskId = taskContext.TaskId,
                StartTime = DateTime.Now,
                Duration = 0,
                State = (int) InstanceState.InProcess
            };

            dtoTaskInstance.Id =
                repository.CreateEntity(dtoTaskInstance);
            try
            {
                foreach (var oper in opers)
                {
                    var dtoOperInstance = new DtoOperInstance
                    {
                        TaskInstanceId = dtoTaskInstance.Id,
                        OperationId = oper.Id,
                        StartTime = DateTime.Now,
                        Duration = 0,
                        State = (int) InstanceState.InProcess
                    };

                    dtoOperInstance.Id =
                        repository.CreateEntity(dtoOperInstance);

                    Stopwatch operDuration = new Stopwatch();
                    operDuration.Start();

                    switch (oper)
                    {
                        case IDataImporter importer:
                            try
                            {
                                importer.Execute(taskContext);

                                dtoOperInstance.DataSet = archiver.CompressString(taskContext
                                    .DataSets[importer.DataSetName]);
                                dtoOperInstance.State = (int) InstanceState.Success;
                                operDuration.Stop();
                                dtoOperInstance.Duration =
                                    Convert.ToInt32(operDuration.ElapsedMilliseconds);
                                repository.UpdateEntity(dtoOperInstance);
                            }
                            catch (Exception e)
                            {
                                exceptions.Add(new Tuple<Exception, string>(e, importer.Name));
                                dtoOperInstance.ErrorMessage = e.Message;
                                dtoOperInstance.State = (int) InstanceState.Failed;
                                operDuration.Stop();
                                dtoOperInstance.Duration =
                                    Convert.ToInt32(operDuration.ElapsedMilliseconds);
                                repository.UpdateEntity(dtoOperInstance);
                            }

                            break;

                        case IDataExporter exporter:

                            try
                            {
                                exporter.Send(taskContext);
                                dtoOperInstance.State = (int) InstanceState.Success;
                                operDuration.Stop();
                                dtoOperInstance.Duration =
                                    Convert.ToInt32(operDuration.ElapsedMilliseconds);
                                repository.UpdateEntity(dtoOperInstance);
                            }
                            catch (Exception e)
                            {
                                exceptions.Add(new Tuple<Exception, string>(e, exporter.Name));
                                dtoOperInstance.ErrorMessage = e.Message;
                                dtoOperInstance.State = (int) InstanceState.Failed;
                                operDuration.Stop();
                                dtoOperInstance.Duration =
                                    Convert.ToInt32(operDuration.ElapsedMilliseconds);
                                repository.UpdateEntity(dtoOperInstance);
                            }

                            break;
                    }
                }

                if (exceptions.Count == 0)
                {
                    var msg = $"Задача {taskContext.TaskId} успешно выполнена";
                    monik.ApplicationInfo(msg);
                    Console.WriteLine(msg);
                }

                else
                {
                    success = false;
                    taskContext.exporter.SendError(exceptions, taskContext.TaskName);
                    var msg = $"Задача {taskContext.TaskId} выполнена с ошибками";
                    monik.ApplicationInfo(msg);
                    Console.WriteLine(msg);
                }
            }

            catch (Exception e)
            {
                success = false;
                taskContext.exporter.SendError(exceptions, taskContext.TaskName);
                var msg = $"Задача {taskContext.TaskId} не выполнена.Возникла ошибка: {e.Message}";
                monik.ApplicationError(msg);
                Console.WriteLine(msg);
            }

            duration.Stop();

            dtoTaskInstance.Duration = Convert.ToInt32(duration.ElapsedMilliseconds);

            dtoTaskInstance.State =
                success ? (int) InstanceState.Success : (int) InstanceState.Failed;

            repository.UpdateEntity(mapper.Map<DtoTaskInstance>(dtoTaskInstance));
        }

        public async Task<string> RunOperationsAndGetLastView(
            List<IOperation> opers, IRTaskRunContext taskContext)
        {
            await System.Threading.Tasks.Task.Factory.StartNew(() =>
                RunOperations(opers, taskContext));

            var val = taskContext.DataSets.LastOrDefault().Value;

            return taskContext.exporter.GetDefaultView(taskContext.TaskName, val);
        }

        public async void RunOperationsAndSendLastView(List<IOperation> opers,
                                                       IRTaskRunContext taskContext,
                                                       string mailAddress)
        {
            var view = await RunOperationsAndGetLastView(opers, taskContext);

            if (string.IsNullOrEmpty(view)) return;

            taskContext.exporter.ForceSend(view, taskContext.TaskName, mailAddress);
        }
    }
}