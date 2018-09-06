using Autofac;
using AutoMapper;
using Monik.Client;
using ReportService.Interfaces;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace ReportService.Core
{
    public class RTask : IRTask
    {
        public int Id { get; }
        public string ReportName { get; }
        public string ViewTemplate { get; }
        public DtoSchedule Schedule { get; }
        public string ConnectionString { get; }
        public string Query { get; }
        public int TryCount { get; }
        public int QueryTimeOut { get; }
        public RReportType Type { get; }
        public int ReportId { get; }
        public bool HasHtmlBody { get; }
        public bool HasJsonEn { get; }
        public bool HasXlsx { get; }
        public bool HasTelegramView { get; }
        public DateTime LastTime { get; private set; }
        public List<IDataExporter> Exporters { get; set; }

        private readonly IRepository repository;
        private readonly IClientControl monik;
        private readonly IMapper mapper;
        private readonly IArchiver archiver;

        public RTask(ILifetimeScope autofac, IRepository repository,
                     IClientControl monik, IMapper mapper, IArchiver archiver,
                     int id, string reportName, string template, DtoSchedule schedule,
                     string connStr, string query,
                     int tryCount, int timeOut, RReportType reportType, int reportId,
                     List<DtoOper> dataExporterConfigs)
        {
            Type = reportType;

            //switch (Type)
            //{
            //    case RReportType.Common:
            //        dataEx = autofac.ResolveNamed<IDataExecutor>("commondataex");
            //        viewEx = autofac.ResolveNamed<IViewExecutor>("commonviewex");
            //        break;
            //    case RReportType.Custom:
            //        dataEx = autofac.ResolveNamed<IDataExecutor>(query);
            //        viewEx = autofac.ResolveNamed<IViewExecutor>(template);
            //        break;
            //    default:
            //        throw new NotImplementedException();
            //}

            this.archiver = archiver;
            this.monik = monik;
            this.mapper = mapper;
            Id = id;
            Exporters=new List<IDataExporter>();

           foreach (var config in dataExporterConfigs)
                Exporters.Add(autofac.ResolveNamed<IDataExporter>(config.Type,
                   new NamedParameter("jsonConfig", config.Name)));

            ReportName = reportName;
            Query = query;
            ViewTemplate = template;
            ReportId = reportId;
            Schedule = schedule;
            this.repository = repository;
            TryCount = tryCount;
            QueryTimeOut = timeOut;
            ConnectionString = connStr;

        }

        public void Execute(string address = null)
        {
            var dtoInstance = new DtoTaskInstance()
            {
                StartTime = DateTime.Now,
                TaskId = Id,
                State = (int) InstanceState.InProcess
            };

            dtoInstance.Id =
                repository.CreateEntity(mapper.Map<DtoTaskInstance>(dtoInstance));

            repository.CreateEntity(mapper.Map<DtoOperInstance>(dtoInstance));

            Stopwatch duration = new Stopwatch();
            duration.Start();
            int i = 1;
            bool dataObtained = false;

            var sendData = "";

            while (!dataObtained && i <= TryCount)
            {
                try
                {
                   // sendData = dataEx.Execute(this);
                    dataObtained = true;
                    i++;
                    break;
                }
                catch (Exception ex)
                {
                    sendData = ex.Message;
                }

                i++;
            }

            if (dataObtained)
            {
                if (HasHtmlBody)
                    try
                    {
                      //  sendData = viewEx.ExecuteHtml(ViewTemplate, sendData);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                        sendData = ex.Message;
                    }

                if (HasTelegramView)
                    try
                    {
                       // sendData =
                       //     viewEx.ExecuteTelegramView(sendData, ReportName);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                        sendData = ex.Message;
                    }

                if (HasXlsx)
                    try
                    {
                     //   var t = viewEx.ExecuteXlsx(sendData, ReportName);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                        sendData = null;
                    }

                try
                {
                    foreach (var exporter in Exporters)
                        exporter.Send(sendData);
                }

                catch (Exception e)
                {
                    monik.ApplicationError("Произошла ошибка: " + e.Message);
                    Console.WriteLine("Произошла ошибка: "      + e.Message);
                }
                finally
                {
                    if (HasXlsx)
                        //sendData.XlsxData?.Dispose();
                    monik.ApplicationInfo($"Отчёт {Id} успешно выслан");
                    Console.WriteLine($"Отчёт {Id} успешно выслан");
                }
            }


            duration.Stop();
         //   dtoInstance.Data = archiver.CompressString(sendData.JsonBaseData);
         //   dtoInstance.ViewData = archiver.CompressString(sendData.HtmlData);
            dtoInstance.Duration = Convert.ToInt32(duration.ElapsedMilliseconds);
            dtoInstance.State =
                dataObtained ? (int) InstanceState.Success : (int) InstanceState.Failed;

            repository.UpdateEntity(mapper.Map<DtoTaskInstance>(dtoInstance));
            repository.UpdateEntity(mapper.Map<DtoOperInstance>(dtoInstance));
        } //method

        public string GetCurrentView()
        {
            int i = 1;
            bool dataObtained = false;
            string htmlReport = "";

            while (!dataObtained && i <= TryCount)
            {
                try
                {
                 //   var jsonReport = dataEx.Execute(this);
                  //  htmlReport = viewEx.ExecuteHtml(ViewTemplate, jsonReport);
                    dataObtained = true;
                    i++;
                    break;
                }
                catch (Exception ex)
                {
                    htmlReport = ex.Message;
                }

                i++;
            }

            return htmlReport;
        }

        public void UpdateLastTime()
        {
            LastTime = DateTime.Now;
        }
    } //class
}