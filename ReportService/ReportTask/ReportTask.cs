using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Autofac;
using Monik.Common;
using Newtonsoft.Json;
using ReportService.Entities;
using ReportService.Entities.Dto;
using ReportService.Extensions;
using ReportService.Interfaces.Core;
using ReportService.Interfaces.Operations;
using ReportService.Interfaces.ReportTask;

namespace ReportService.ReportTask
{
    public partial class ReportTask : IReportTask
    {
        public int Id { get; }
        public string Name { get; }
        public DtoSchedule Schedule { get; }
        public DateTime LastTime { get; private set; }
        public List<IOperation> Operations { get; set; } = new List<IOperation>();
        public Dictionary<string, object> Parameters { get; set; } = new Dictionary<string, object>();//todo: change to class?
        public List<ParameterInfo> ParameterInfos { get; set; } = new List<ParameterInfo>();
        public List<TaskDependency> DependsOn { get; set; } = new List<TaskDependency>();

        private readonly IMonik mMonik;
        private readonly ILifetimeScope mAutofac;
        private readonly IRepository mRepository;

        public ReportTask(ILogic logic, ILifetimeScope autofac, IRepository repository, IMonik monik, int id,
            string name, string parameters, string dependsOn, DtoSchedule schedule, List<DtoOperation> operations, string parameterInfos)
        {
            mMonik = monik;
            mRepository = repository;
            Id = id;
            Name = name;
            Schedule = schedule;
            
            if (!string.IsNullOrEmpty(parameters))
                Parameters = JsonConvert.DeserializeObject<Dictionary<string, object>>(parameters);

            if (!string.IsNullOrEmpty(dependsOn))
                DependsOn = JsonConvert.DeserializeObject<List<TaskDependency>>(dependsOn);

            if (!string.IsNullOrEmpty(parameterInfos))
                ParameterInfos = JsonConvert.DeserializeObject<List<ParameterInfo>>(parameterInfos);

            mAutofac = autofac;

            ParseDtoOperations(logic, operations);
        } //ctor

        private void ParseDtoOperations(ILogic logic, List<DtoOperation> operations)
        {
            foreach (var dtoOperation in operations)
            {
                IOperation operation;

                var operationType = dtoOperation.ImplementationType;

                if (logic.RegisteredImporters.ContainsKey(operationType))
                {
                    operation = mAutofac.ResolveNamed<IOperation>(operationType, new NamedParameter("config",
                        JsonConvert.DeserializeObject(dtoOperation.Config, logic.RegisteredImporters[operationType])));
                }

                else
                {
                    operation = mAutofac.ResolveNamed<IOperation>(operationType, new NamedParameter("config",
                            JsonConvert.DeserializeObject(dtoOperation.Config, logic.RegisteredExporters[operationType])));
                }

                if (operation == null) continue;

                operation.Properties.Id = dtoOperation.Id;
                operation.Properties.Number = dtoOperation.Number;
                operation.Properties.Name = dtoOperation.Name;
                operation.Properties.IsDefault = dtoOperation.IsDefault;

                Operations.Add(operation);
            }
        }

        public IReportTaskRunContext GetCurrentContext(bool takeDefault)
        {
            var context = mAutofac.Resolve<IReportTaskRunContext>();

            try
            {
                context.OpersToExecute = takeDefault
                    ? Operations.Where(operation => operation.Properties.IsDefault).OrderBy(operation => operation.Properties.Number).ToList()
                    : Operations.OrderBy(operation => operation.Properties.Number).ToList();

                if (!context.OpersToExecute.Any())
                {
                    var msg = $"Task {Id} did not executed (no operations found)";
                    mMonik.ApplicationInfo(msg);
                    Console.WriteLine(msg);
                    return null;
                }

                context.DefaultExporter = mAutofac.Resolve<IDefaultTaskExporter>();
                context.TaskId = Id;
                context.TaskName = Name;
                context.DependsOn = DependsOn;

                context.CancelSource = new CancellationTokenSource();


                var pairsTask = Task.Run(async () => await Task.WhenAll(Parameters.Select
                    (async pair => new KeyValuePair<string, object>(pair.Key, await mRepository.GetBaseQueryResult("select " + pair.Value, context.CancelSource.Token)))));

                var pairs = pairsTask.Result;

                context.Parameters = pairs.ToDictionary(pair => pair.Key, pair => pair.Value);

                var dtoTaskInstance = new DtoTaskInstance
                {
                    TaskId = Id,
                    StartTime = DateTime.Now,
                    Duration = 0,
                    State = (int)InstanceState.InProcess
                };

                dtoTaskInstance.Id = mRepository.CreateEntity(dtoTaskInstance);

                context.TaskInstance = dtoTaskInstance;

                return context;
            }
            catch (Exception ex)
            {
                var exceptions = new List<Tuple<Exception, string>>();
                var allExceptions = ex.FromHierarchy(e => e.InnerException).ToList();

                exceptions.AddRange(allExceptions.Select(exx => new Tuple<Exception, string>(exx, context.TaskName)));
                context.DefaultExporter.SendError(exceptions, context.TaskName, context.TaskId);
			}
            return null;
        }
        public void Execute(IReportTaskRunContext context)
        {
            var taskWorker = mAutofac.Resolve<ITaskWorker>();
            taskWorker.RunTask(context);
        }

        public async Task<string> GetCurrentViewAsync(IReportTaskRunContext context)
        {
            var taskWorker = mAutofac.Resolve<ITaskWorker>();

            var defaultView = await taskWorker.RunTaskAndGetLastViewAsync(context);

            return string.IsNullOrEmpty(defaultView)
                ? null
                : defaultView;
        }

        public void SendDefault(IReportTaskRunContext context, string mailAddress)
        {
            var taskWorker = mAutofac.Resolve<ITaskWorker>();

            taskWorker.RunTaskAndSendLastViewAsync(context, mailAddress);
        }

        public void UpdateLastExecutionTime()
        {
            var taskState = mRepository.GetTaskStateById(Id);
            LastTime = taskState.LastStart;
        }
    } //class
}
