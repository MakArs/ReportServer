using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Monik.Common;
using ReportService.Entities;
using ReportService.Entities.Dto;
using ReportService.Extensions;
using ReportService.Interfaces.Core;
using ReportService.Interfaces.Operations;
using ReportService.Interfaces.ReportTask;

namespace ReportService.ReportTask
{
    public class TaskWorker : ITaskWorker
    {
        private readonly IRepository mRepository;
        private readonly IMonik mMonik;
        private int mDependenciesWaitingCount = 3;
        private readonly int mDependenciesWaitingSeconds = 300;

        public TaskWorker(IRepository repository, IMonik monik)
        {
            mRepository = repository;
            mMonik = monik;
        }

        private string GetOperationStateFromInstance(string operName, DtoOperInstance instance)
        {
            var state = (InstanceState)instance.State;

            return operName +
                   $" (State: {state.ToString()}," +
                   $" started: {instance.StartTime}" +
                   (instance.Duration > 0
                       ? $", duration: {instance.Duration / 60000} m {(instance.Duration % 60000) / 1000.0:f0} s"
                       : "")
                   + ")";
        }

        private void ProcessNotExecutedTask(IReportTaskRunContext taskContext,
            List<Tuple<Exception, string>> exceptions, Exception e)
        {

            var oper = taskContext.OpersToExecute.First();

            exceptions.Add(new Tuple<Exception, string>(e, oper.Properties.Name));

            var msg = $"Task {taskContext.TaskId} was not executed (" + e.Message + ")";

            SendServiceInfo(msg);

            taskContext.DefaultExporter.SendError(exceptions, taskContext.TaskName, taskContext.TaskId);

            taskContext.TaskInstance.Duration = 0;

            taskContext.TaskInstance.State =
                (int)InstanceState.Failed;

            var dtoOperInstance = new DtoOperInstance
            {
                TaskInstanceId = taskContext.TaskInstance.Id,
                OperationId = oper.Properties.Id,
                StartTime = DateTime.Now,
                Duration = 0,
                ErrorMessage = e.Message,
                State = (int)InstanceState.Failed
            };

            dtoOperInstance.Id =
                mRepository.CreateEntity(dtoOperInstance);

            mRepository.UpdateEntity(taskContext.TaskInstance);
        }

        private bool CheckIfDependenciesCompleted(IReportTaskRunContext taskContext,
            List<Tuple<Exception, string>> exceptions)
        {
            try
            {
                if (taskContext.DependsOn.Count == 0)
                    return true;

                string unCompletedDependencies;

                do
                {
                    var dependsOnStates = taskContext.DependsOn
                        .Select(dependency =>
                        {
                            var state = mRepository.GetTaskStateById(dependency.TaskId);
                            return new
                            {
                                dependency.TaskId,
                                dependency.MaxSecondsPassed,
                                SecondsPassed = (DateTime.Now - state.LastSuccessfulFinish).TotalSeconds,
                                state.InProcessCount
                            };
                        }).ToList();

                    if (dependsOnStates.Any(state => state.InProcessCount > 0))
                    {
                        var waitInterval = Math.Min(mDependenciesWaitingSeconds,
                                               taskContext.DependsOn.Select(dep => dep.MaxSecondsPassed).Min() - 60) *
                                           1000;

                        Task.Delay(waitInterval > 0
                            ? waitInterval
                            : 1000).Wait(taskContext.CancelSource.Token);
                    }

                    unCompletedDependencies = string.Join(", ",
                        dependsOnStates.Where(state => state.SecondsPassed > state.MaxSecondsPassed)
                            .Select(state => state.TaskId));

                    mDependenciesWaitingCount--;
                } while (!string.IsNullOrEmpty(unCompletedDependencies)
                         && mDependenciesWaitingCount > 0);

                if (!string.IsNullOrEmpty(unCompletedDependencies))
                {
                    var msg = $"uncompleted dependencies: {unCompletedDependencies}";
                    throw new Exception(msg);
                }

                return true;
            }

            catch (Exception e)
            {
                ProcessNotExecutedTask(taskContext, exceptions, e);

                return false;
            }
        }

        public void RunTask(IReportTaskRunContext taskContext)
        {
            Stopwatch duration = new Stopwatch();


            duration.Start();

            bool deleteFolder = false;

            if (taskContext.OpersToExecute.Any(oper => oper.CreateDataFolder))
            {
                deleteFolder = true;
                taskContext.CreateDataFolder();
            }

            taskContext.PackageStates = taskContext.OpersToExecute
                .Select(oper => oper.Properties.Name + " (Not started) ").ToList();

            var dtoTaskInstance = taskContext.TaskInstance;

            var taskRequestInfo = taskContext.TaskRequestInfo;

            var success = true;
            var exceptions = new List<Tuple<Exception, string>>();

            if (taskRequestInfo != null)
            {
                taskRequestInfo.Status = (int)RequestStatus.InProgress;
                taskRequestInfo.TaskInstanceId = dtoTaskInstance.Id;
                taskRequestInfo.UpdateTime = DateTime.UtcNow;
                mRepository.UpdateEntity(taskRequestInfo);
            }

            if (!CheckIfDependenciesCompleted(taskContext, exceptions))
                return;

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

                    RunOperation(taskContext, oper, dtoTaskInstance, exceptions);
                }

                if (exceptions.Count == 0 || dtoTaskInstance.State == (int)InstanceState.Canceled)
                {
                    var msg = dtoTaskInstance.State == (int)InstanceState.Canceled
                        ? $"Task {taskContext.TaskId} stopped"
                        : $"Task {taskContext.TaskId} completed successfully";
                    SendServiceInfo(msg);
                }

                else
                {
                    success = false;
                    var msg = $"Task {taskContext.TaskId} completed with errors";
                    SendServiceInfo(msg);

                    taskContext.DefaultExporter.SendError(exceptions, taskContext.TaskName, taskContext.TaskId);
                }
            }

            catch (Exception e)
            {
                success = false;
                var msg = $"Task {taskContext.TaskId}, named {taskContext.TaskName} is not completed. An error has occurred: {e.Message}";
                mMonik.ApplicationError(msg);
                Console.WriteLine(msg);
                //  TODO: add a taskId to mail theme.
                taskContext.DefaultExporter.SendError(exceptions, taskContext.TaskName, taskContext.TaskId);
            }

            duration.Stop();

            if (deleteFolder)
                taskContext.RemoveDataFolder();

            dtoTaskInstance.Duration = Convert.ToInt32(duration.ElapsedMilliseconds);

            dtoTaskInstance.State =
                success ? (int)InstanceState.Success
                : dtoTaskInstance.State == (int)InstanceState.Canceled ? (int)InstanceState.Canceled
                : (int)InstanceState.Failed;

            mRepository.UpdateEntity(dtoTaskInstance);

            if (taskRequestInfo != null)
            {
                taskRequestInfo.Status =
                    success ? (int)RequestStatus.Completed
                    : taskRequestInfo.Status == (int)RequestStatus.Canceled ? (int)RequestStatus.Canceled
                    : (int)RequestStatus.Failed;

                taskRequestInfo.UpdateTime = DateTime.UtcNow;

                mRepository.UpdateEntity(taskRequestInfo);
            }
        }

        private void RunOperation(IReportTaskRunContext taskContext, IOperation oper,
            DtoTaskInstance dtoTaskInstance, List<Tuple<Exception, string>> exceptions)
        {
            {
                var dtoOperInstance = new DtoOperInstance
                {
                    TaskInstanceId = dtoTaskInstance.Id,
                    OperationId = oper.Properties.Id,
                    StartTime = DateTime.Now,
                    Duration = 0,
                    State = (int)InstanceState.InProcess
                };

                dtoOperInstance.Id =
                    mRepository.CreateEntity(dtoOperInstance);

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

                    dtoOperInstance.State = (int)InstanceState.Success;
                    operDuration.Stop();
                    dtoOperInstance.Duration =
                        Convert.ToInt32(operDuration.ElapsedMilliseconds);
                    mRepository.UpdateEntity(dtoOperInstance);
                }

                catch (Exception e)
                {
                    if (e is OperationCanceledException)
                        dtoOperInstance.State = (int)InstanceState.Canceled;

                    else
                    {
                        if (e.InnerException == null)
                        {
                            exceptions.Add(new Tuple<Exception, string>(e, oper.Properties.Name));
                            dtoOperInstance.ErrorMessage = e.Message;
                        }

                        else
                        {
                            var allExceptions = e.GetExceptionTree().ToList();

                            exceptions.AddRange(allExceptions
                                .Select(exx => new Tuple<Exception, string>(exx, oper.Properties.Name)));

                            dtoOperInstance.ErrorMessage =
                                string.Join("\n", allExceptions.Select(exx => exx.Message));
                        }

                        dtoOperInstance.State = (int)InstanceState.Failed;
                    }

                    operDuration.Stop();
                    dtoOperInstance.Duration =
                        Convert.ToInt32(operDuration.ElapsedMilliseconds);
                    mRepository.UpdateEntity(dtoOperInstance);
                }
                finally
                {
                    taskContext.PackageStates[oper.Properties.Number - 1] =
                        GetOperationStateFromInstance(oper.Properties.Name, dtoOperInstance);
                }
            }
        }

        public async Task<string> RunTaskAndGetLastViewAsync(IReportTaskRunContext taskContext)
        {
            await Task.Factory.StartNew(() =>
                RunTask(taskContext));

            var val = taskContext.Packages.LastOrDefault().Value;

            return taskContext.DefaultExporter.GetDefaultPackageView(taskContext.TaskName, val);
        }

        public async Task RunTaskAndSendLastViewAsync(IReportTaskRunContext taskContext,
            string mailAddress)
        {
            var view = await RunTaskAndGetLastViewAsync(taskContext);

            if (string.IsNullOrEmpty(view)) return;

            taskContext.DefaultExporter.ForceSend(view, taskContext.TaskName, mailAddress);
        }

        private void SendServiceInfo(string msg)
        {
            mMonik.ApplicationInfo(msg);
            Console.WriteLine(msg);
        }

    }
}
