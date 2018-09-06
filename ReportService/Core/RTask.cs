using Autofac;
using AutoMapper;
using Monik.Client;
using ReportService.Interfaces;
using System;
using System.Collections.Generic;
using System.Diagnostics;

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

        private readonly IRepository repository;
        private readonly IClientControl monik;
        private readonly IMapper mapper;
        private readonly IArchiver archiver;

        public RTask(ILifetimeScope autofac, IRepository repository,
                     IClientControl monik, IMapper mapper, IArchiver archiver, int id,
                     string name, DtoSchedule schedule, List<DtoOper> opers)
        {
            this.archiver = archiver;
            this.monik = monik;
            this.mapper = mapper;
            Id = id;
            Name = name;
            Schedule = schedule;
            DataSets = new Dictionary<string, string>();
            Operations = new List<IOperation>();

            foreach (var oper in opers)
            {
                var newOper = autofac.ResolveNamed<IDataExporter>(oper.Name,
                    new NamedParameter("jsonConfig", oper.Config));
                newOper.Id = oper.Id;
                Operations.Add(newOper);
            }

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
                foreach (var oper in Operations)
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
                                DataSets.Add(importer.DataSetName, newDataSet);

                                dtoOperInstance.DataSet = archiver.CompressString(newDataSet);
                                dtoOperInstance.State = (int) InstanceState.Success;
                                operDuration.Stop();
                                dtoOperInstance.Duration =
                                    Convert.ToInt32(operDuration.ElapsedMilliseconds);
                                repository.UpdateEntity(dtoOperInstance);
                            }
                            catch (Exception e)
                            {
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

                monik.ApplicationInfo($"Задача {Id} успешно выполнена");
                Console.WriteLine($"Задача {Id} успешно выполнена");
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

        public string GetCurrentView()
        {
            int i = 1;
            bool dataObtained = false;
            string htmlReport = "";

            try
            {
                //   var jsonReport = dataEx.Execute(this);
                //  htmlReport = viewEx.ExecuteHtml(ViewTemplate, jsonReport);
                dataObtained = true;
                i++;
            }
            catch (Exception ex)
            {
                htmlReport = ex.Message;
            }

            i++;

            return ""; //htmlReport;
        }

        public void UpdateLastTime()
        {
            LastTime = DateTime.Now;
        }
    } //class
}