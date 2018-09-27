using Autofac;
using AutoMapper;
using ReportService.Interfaces;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Monik.Common;
using Newtonsoft.Json;

namespace ReportService.Core
{
    public class RTask : IRTask
    {
        public int Id { get; }
        public string Name { get; }
        public DtoSchedule Schedule { get; }
        public DateTime LastTime { get; private set; }
        public List<IOperation> Operations { get; set; }

        private readonly DefaultTaskWorker worker;
        private readonly IRepository repository;
        private readonly IMonik monik;
        private readonly IMapper mapper;
        private readonly IArchiver archiver;
        private string defaultView;

        public RTask(ILogic logic, ILifetimeScope autofac, IRepository repository,
                     IMonik monik, IMapper mapper, IArchiver archiver, int id,
                     string name, DtoSchedule schedule, List<Tuple<DtoOper, int, bool>> opers)
        {
            this.archiver = archiver;
            this.monik = monik;
            this.mapper = mapper;
            Id = id;
            Name = name;
            Schedule = schedule;
            Operations = new List<IOperation>();

            foreach (var operTuple in opers)
            {
                IOperation newOper;
                var oper = operTuple.Item1;
                var operType = oper.Type;

                if (logic.RegisteredImporters.ContainsKey(operType))
                {
                    newOper = autofac.ResolveNamed<IDataImporter>(operType,
                        new NamedParameter("config",
                            JsonConvert.DeserializeObject(oper.Config,
                                logic.RegisteredImporters[operType])));
                }

                else
                {
                    newOper = autofac.ResolveNamed<IDataExporter>(operType,
                        new NamedParameter("config",
                            JsonConvert.DeserializeObject(oper.Config,
                                logic.RegisteredExporters[operType])));
                }

                if (newOper == null) continue;

                newOper.Id = oper.Id;
                newOper.Number = operTuple.Item2;
                newOper.Name = oper.Name;
                newOper.IsDefault = operTuple.Item3;

                Operations.Add(newOper);
            }

            worker = autofac.Resolve<DefaultTaskWorker>
                (new NamedParameter("task", this));

            this.repository = repository;
        }

        public void Execute(bool useDefault = false)
        {
            var dtoTaskInstance = new DtoTaskInstance
            {
                TaskId = Id,
                StartTime = DateTime.Now,
                Duration = 0,
                State = (int) InstanceState.InProcess
            };

            dtoTaskInstance.Id =
                repository.CreateEntity(dtoTaskInstance);

            Stopwatch duration = new Stopwatch();
            duration.Start();
            var success = true;

            try
            {
                var exceptions = new List<Tuple<Exception, string>>();
                List<IOperation> opersToExecute;
                Dictionary<string, string> dataSets = new Dictionary<string, string>();

                lock (this)
                    opersToExecute = useDefault
                        ? Operations.Where(oper => oper.IsDefault)
                            .OrderBy(oper => oper.Number).ToList()
                        : Operations.OrderBy(oper => oper.Number).ToList();

                if (!opersToExecute.Any())
                {
                    monik.ApplicationInfo($"Задача {Id} не выполнена (не заданы операции)");
                    Console.WriteLine($"Задача {Id} не выполнена (не заданы операции)");
                    return;
                }

                foreach (var oper in opersToExecute)
              {
                    var dtoOperInstance = new DtoOperInstance
                    {
                        TaskInstanceId = dtoTaskInstance.Id,
                        OperId = oper.Id,
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
                                var newDataSet = importer.Execute();
                                lock (this)
                                    dataSets[importer.DataSetName] = newDataSet;

                                dtoOperInstance.DataSet = archiver.CompressString(newDataSet);
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
                                exporter.Send(dataSets[exporter.DataSetName]);
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
                    if (useDefault)
                        defaultView = worker.GetDefaultView(Name, dataSets.Last().Value);
                    monik.ApplicationInfo($"Задача {Id} успешно выполнена");
                    Console.WriteLine($"Задача {Id} успешно выполнена");
                }

                else
                {
                    success = false;
                    worker.SendError(exceptions, Name);
                    monik.ApplicationInfo($"Задача {Id} выполнена с ошибками");
                    Console.WriteLine($"Задача {Id} выполнена с ошибками");
                }
            }

            catch (Exception e)
            {
                success = false;
                monik.ApplicationError($"Задача {Id} не выполнена.Возникла ошибка: {e.Message}");
                Console.WriteLine($"Задача {Id} не выполнена.Возникла ошибка: {e.Message}");
            }

            duration.Stop();
            dtoTaskInstance.Duration = Convert.ToInt32(duration.ElapsedMilliseconds);
            dtoTaskInstance.State =
                success ? (int) InstanceState.Success : (int) InstanceState.Failed;

            repository.UpdateEntity(mapper.Map<DtoTaskInstance>(dtoTaskInstance));
        } //method

        public async Task<string> GetCurrentView()
        {
            await Task.Factory.StartNew(() => Execute(true));
            return string.IsNullOrEmpty(defaultView)
                ? "This task has not default operations.."
                : defaultView;
        }

        public void SendDefault(string mailAddress)
        {
            Execute(true);
            if (string.IsNullOrEmpty(defaultView)) return;

            worker.ForceSend(defaultView, Name, mailAddress);
        }

        public void UpdateLastTime()
        {
            LastTime = DateTime.Now;
        }
    } //class
}