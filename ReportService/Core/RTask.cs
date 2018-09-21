using Autofac;
using AutoMapper;
using ReportService.Interfaces;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
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
        public Dictionary<string, string> DataSets { get; }
        public List<IOperation> Operations { get; set; }

        private readonly DefaultTaskWorker worker;
        private readonly IRepository repository;
        private readonly IMonik monik;
        private readonly IMapper mapper;
        private readonly IArchiver archiver;
        private readonly int defaultImporter;

        public RTask(ILogic logic, ILifetimeScope autofac, IRepository repository,
                     IMonik monik, IMapper mapper, IArchiver archiver, int id,
                     string name, DtoSchedule schedule, List<Tuple<DtoOper,int>> opers)
        {
            this.archiver = archiver;
            this.monik = monik;
            this.mapper = mapper;
            Id = id;
            Name = name;
            Schedule = schedule;
            DataSets = new Dictionary<string, string>();
            Operations = new List<IOperation>();

           // defaultEmailSender = autofac.ResolveNamed<IDataExporter>("");
            foreach (var opern in opers)
            {
                IOperation newOper;
                var oper = opern.Item1;
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

                if (newOper != null)
                {
                    newOper.Id = oper.Id;
                    newOper.Number = opern.Item2;
                    newOper.Name = oper.Name;

                    Operations.Add(newOper);
                }
            }

            worker = autofac.Resolve<DefaultTaskWorker>();
            this.repository = repository;
        }

        public void Execute()
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

                foreach (var oper in Operations.OrderBy(oper => oper.Number))
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
                                    DataSets[importer.DataSetName] = newDataSet;

                                dtoOperInstance.DataSet = archiver.CompressString(newDataSet);
                                dtoOperInstance.State = (int) InstanceState.Success;
                                operDuration.Stop();
                                dtoOperInstance.Duration =
                                    Convert.ToInt32(operDuration.ElapsedMilliseconds);
                                repository.UpdateEntity(dtoOperInstance);
                            }
                            catch (Exception e)
                            {
                                exceptions.Add(new Tuple<Exception, string>(e,importer.Name));
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
                                exporter.Send(DataSets[exporter.DataSetName]);
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
                    monik.ApplicationInfo($"Задача {Id} успешно выполнена");
                    Console.WriteLine($"Задача {Id} успешно выполнена");
                }

                else
                {
                    worker.SendError(exceptions,Name);
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

        public string GetCurrentView() //todo: remake method with new db conception
        {
            if (defaultImporter == 0 || !(Operations[defaultImporter] is IDataImporter imp))
                return null;

            return worker.GetDefaultView(imp.Execute(), Name);
        }

        public void UpdateLastTime()
        {
            LastTime = DateTime.Now;
        }
    } //class
}