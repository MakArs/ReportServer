using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Autofac;
using Monik.Common;
using Newtonsoft.Json;
using ReportService.Interfaces.Core;
using ReportService.Interfaces.ReportTask;

namespace ReportService.ReportTask
{
    public class RTask : IRTask
    {
        public int Id { get; }
        public string Name { get; }
        public DtoSchedule Schedule { get; }
        public DateTime LastTime { get; private set; }
        public List<IOperation> Operations { get; set; }

        private readonly IMonik monik;
        private readonly ILifetimeScope autofac;

        public RTask(ILogic logic, ILifetimeScope autofac,
                     IMonik monik, int id,
                     string name, DtoSchedule schedule, List<DtoOperation> opers)
        {
            this.monik = monik;
            Id = id;
            Name = name;
            Schedule = schedule;
            Operations = new List<IOperation>();

            foreach (var operation in opers)
            {
                IOperation newOper;

                var operType = operation.ImplementationType;

                if (logic.RegisteredImporters.ContainsKey(operType))
                {
                    newOper = autofac.ResolveNamed<IDataImporter>(operType,
                        new NamedParameter("config",
                            JsonConvert.DeserializeObject(operation.Config,
                                logic.RegisteredImporters[operType])));
                }

                else
                {
                    newOper = autofac.ResolveNamed<IDataExporter>(operType,
                        new NamedParameter("config",
                            JsonConvert.DeserializeObject(operation.Config,
                                logic.RegisteredExporters[operType])));
                }

                if (newOper == null) continue;

                newOper.Id = operation.Id;
                newOper.Number = operation.Number;
                newOper.Name = operation.Name;
                newOper.IsDefault = operation.IsDefault;

                Operations.Add(newOper);
            }

            this.autofac = autofac;
        } //ctor

        public void Execute()
        {
            var context = autofac.Resolve<IRTaskRunContext>();
            context.exporter = autofac.Resolve<IDefaultTaskExporter>();
            context.TaskId = Id;
            context.TaskName = Name; //can do it by NamedParameter+ctor,but..

            var opersToExecute = Operations.OrderBy(oper => oper.Number).ToList();

            if (!opersToExecute.Any())
            {
                var msg = $"Task {Id} did not executed (no operations found)";
                monik.ApplicationInfo(msg);
                Console.WriteLine(msg);
                return;
            }

            var taskWorker = autofac.Resolve<ITaskWorker>();
            taskWorker.RunOperations(opersToExecute, context);
        }

        public async Task<string> GetCurrentView()
        {
            var opersToExecute = Operations.Where(oper => oper.IsDefault)
                .OrderBy(oper => oper.Number).ToList();

            if (!opersToExecute.Any())
            {
                var msg = $"Task {Id} did not executed (no default operations found)";
                monik.ApplicationInfo(msg);
                Console.WriteLine(msg);
                return null;
            }

            var context = autofac.Resolve<IRTaskRunContext>();
            context.exporter = autofac.Resolve<IDefaultTaskExporter>();
            context.TaskId = Id;
            context.TaskName = Name;

            var taskWorker = autofac.Resolve<ITaskWorker>();

            var defaultView =
                await taskWorker.RunOperationsAndGetLastView(opersToExecute, context);

            return string.IsNullOrEmpty(defaultView)
                ? null
                : defaultView;
        }

        public void SendDefault(string mailAddress)
        {
            var opersToExecute = Operations.Where(oper => oper.IsDefault)
                .OrderBy(oper => oper.Number).ToList();

            if (!opersToExecute.Any())
            {
                var msg = $"Task {Id} did not executed (no default operations found)";
                monik.ApplicationInfo(msg);
                Console.WriteLine(msg);
                return;
            }

            var context = autofac.Resolve<IRTaskRunContext>();
            context.exporter = autofac.Resolve<IDefaultTaskExporter>();
            context.TaskId = Id;
            context.TaskName = Name;

            var taskWorker = autofac.Resolve<ITaskWorker>();

            taskWorker.RunOperationsAndSendLastView(opersToExecute, context, mailAddress);
        }

        public void UpdateLastTime()
        {
            LastTime = DateTime.Now;
        }
    } //class
}