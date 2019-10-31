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
using ReportService.Interfaces.Core;
using ReportService.Interfaces.Operations;
using ReportService.Interfaces.ReportTask;

namespace ReportService.ReportTask
{
    public class ReportTask : IReportTask //todo:mapping for operation props&context
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
            foreach (var operation in opers)
            {
                IOperation newOper;

                var operType = operation.ImplementationType;

                if (logic.RegisteredImporters.ContainsKey(operType))
                {
                    newOper = autofac.ResolveNamed<IOperation>(operType,
                        new NamedParameter("config",
                            JsonConvert.DeserializeObject(operation.Config,
                                logic.RegisteredImporters[operType])));
                }

                else
                {
                    newOper = autofac.ResolveNamed<IOperation>(operType,
                        new NamedParameter("config",
                            JsonConvert.DeserializeObject(operation.Config,
                                logic.RegisteredExporters[operType])));
                }

                if (newOper == null) continue;

                newOper.Properties.Id = operation.Id;
                newOper.Properties.Number = operation.Number;
                newOper.Properties.Name = operation.Name;
                newOper.Properties.IsDefault = operation.IsDefault;

                Operations.Add(newOper);
            }
        }

        public IReportTaskRunContext GetCurrentContext(bool takeDefault)
        {
            var context = autofac.Resolve<IReportTaskRunContext>();

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

            context.Parameters = Parameters
                .ToDictionary(pair => pair.Key,
                    pair => repository.GetBaseQueryResult("select " + pair.Value.ToString()));

            context.CancelSource = new CancellationTokenSource();

            var dtoTaskInstance = new DtoTaskInstance
            {
                TaskId = Id,
                StartTime = DateTime.Now,
                Duration = 0,
                State = (int) InstanceState.InProcess
            };

            dtoTaskInstance.Id =
                repository.CreateEntity<DtoTaskInstance, long>(dtoTaskInstance);

            context.TaskInstance = dtoTaskInstance;

            return context;
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
            LastTime = DateTime.Now;
        }
    } //class
}