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
    public class ReportTask : IReportTask
    {
        public int Id { get; }
        public string Name { get; }
        public DtoSchedule Schedule { get; }
        public DateTime LastTime { get; private set; }
        public List<IOperation> Operations { get; set; }
        public Dictionary<string, object> Parameters { get; set; } //todo: change to class?
        public List<TaskDependency> DependsOn { get; set; }

        private readonly IMonik monik;
        private readonly ILifetimeScope autofac;
        private readonly IRepository repository;

        public ReportTask(ILogic logic, ILifetimeScope autofac, IRepository repository, IMonik monik, int id,
            string name, string parameters, string dependsOn, DtoSchedule schedule, List<DtoOperation> opers)
        {
            this.monik = monik;
            this.repository = repository;
            Id = id;
            Name = name;
            Schedule = schedule;
            Operations = new List<IOperation>();

            Parameters = new Dictionary<string, object>();
            if (!string.IsNullOrEmpty(parameters))
                Parameters = JsonConvert
                    .DeserializeObject<Dictionary<string, object>>(parameters);

            DependsOn = new List<TaskDependency>();
            if (!string.IsNullOrEmpty(dependsOn))
                DependsOn = JsonConvert
                    .DeserializeObject<List<TaskDependency>>(dependsOn);

            this.autofac = autofac;

            ParseDtoOperations(logic, opers);
        } //ctor

        private void ParseDtoOperations(ILogic logic, List<DtoOperation> opers)
        {
            foreach (var dtoOperation in opers)
            {
                IOperation operation;

                var operType = dtoOperation.ImplementationType;

                if (logic.RegisteredImporters.ContainsKey(operType))
                {
                    operation = autofac.ResolveNamed<IOperation>(operType,
                        new NamedParameter("config",
                            JsonConvert.DeserializeObject(dtoOperation.Config,
                                logic.RegisteredImporters[operType])));
                }

                else
                {
                    operation = autofac.ResolveNamed<IOperation>(operType,
                        new NamedParameter("config",
                            JsonConvert.DeserializeObject(dtoOperation.Config,
                                logic.RegisteredExporters[operType])));
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
            var context = autofac.Resolve<IReportTaskRunContext>();

            try
            {
                context.OpersToExecute = takeDefault
                    ? Operations.Where(oper => oper.Properties.IsDefault)
                        .OrderBy(oper => oper.Properties.Number).ToList()
                    : Operations.OrderBy(oper => oper.Properties.Number).ToList();

                if (!context.OpersToExecute.Any())
                {
                    var msg = $"Task {Id} did not executed (no operations found)";
                    monik.ApplicationInfo(msg);
                    Console.WriteLine(msg);
                    return null;
                }

                context.DefaultExporter = autofac.Resolve<IDefaultTaskExporter>();
                context.TaskId = Id;
                context.TaskName = Name;
                context.DependsOn = DependsOn;

                context.CancelSource = new CancellationTokenSource();


                var pairsTask = Task.Run(async () => await Task.WhenAll(Parameters.Select(async pair =>
                    new KeyValuePair<string, object>(pair.Key,
                    await repository.GetBaseQueryResult("select " + pair.Value,
                    context.CancelSource.Token)))));

                var pairs = pairsTask.Result;

                context.Parameters = pairs
                    .ToDictionary(pair => pair.Key, pair => pair.Value);

                var dtoTaskInstance = new DtoTaskInstance
                {
                    TaskId = Id,
                    StartTime = DateTime.Now,
                    Duration = 0,
                    State = (int)InstanceState.InProcess
                };

                dtoTaskInstance.Id =
                    repository.CreateEntity(dtoTaskInstance);

                context.TaskInstance = dtoTaskInstance;

                return context;
            }
            catch (Exception ex)
            {
                var exceptions = new List<Tuple<Exception, string>>();
                var allExceptions = ex.FromHierarchy(e => e.InnerException).ToList();

                exceptions.AddRange(allExceptions
                    .Select(exx => new Tuple<Exception, string>(exx, context.TaskName)));
                context.DefaultExporter.SendError(exceptions, context.TaskName);
			}
            return null;
        }
        public void Execute(IReportTaskRunContext context)
        {
            var taskWorker = autofac.Resolve<ITaskWorker>();
            taskWorker.RunTask(context);
        }

        public async Task<string> GetCurrentViewAsync(IReportTaskRunContext context)
        {
            var taskWorker = autofac.Resolve<ITaskWorker>();

            var defaultView =
                await taskWorker.RunTaskAndGetLastViewAsync(context);

            return string.IsNullOrEmpty(defaultView)
                ? null
                : defaultView;
        }

        public void SendDefault(IReportTaskRunContext context, string mailAddress)
        {
            var taskWorker = autofac.Resolve<ITaskWorker>();

            taskWorker.RunTaskAndSendLastViewAsync(context, mailAddress);
        }

        public void UpdateLastTime()
        {
            var taskState = repository.GetTaskStateById(Id);
            LastTime = taskState.LastStart;
        }
    } //class
}