using Autofac;
using AutoMapper;
using Monik.Client;
using ReportService.Extensions;
using ReportService.Interfaces;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;

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
        public bool HasJsonAttachment { get; }
        public bool HasJsonEnAttachment { get; }
        public bool HasXlsxAttachment { get; }
        public bool HasTelegramView { get; }
        public DateTime LastTime { get; private set; }
        public List<IDataExporter> Exporters { get; set; }

        private readonly IDataExecutor dataEx;
        private readonly IViewExecutor viewEx;
        private readonly IRepository repository;
        private readonly IClientControl monik;
        private readonly IMapper mapper;
        private readonly IArchiver archiver;
        private readonly ITelegramBotClient bot;

        public RTask(ILifetimeScope autofac, IRepository repository,
                     IClientControl monik, IMapper mapper, IArchiver archiver,
                     ITelegramBotClient botClient,
                     int id, string reportName, string template, DtoSchedule schedule,
                     string connStr, string query,
                     int tryCount, int timeOut, RReportType reportType, int reportId,
                     List<DtoExporterConfig> dataExporterConfigs)
        {
            Type = reportType;

            switch (Type)
            {
                case RReportType.Common:
                    dataEx = autofac.ResolveNamed<IDataExecutor>("commondataex");
                    viewEx = autofac.ResolveNamed<IViewExecutor>("commonviewex");
                    break;
                case RReportType.Custom:
                    dataEx = autofac.ResolveNamed<IDataExecutor>(query);
                    viewEx = autofac.ResolveNamed<IViewExecutor>(template);
                    break;
                default:
                    throw new NotImplementedException();
            }

            this.archiver = archiver;
            this.monik = monik;
            this.mapper = mapper;
            bot = botClient;
            Id = id;
            Exporters=new List<IDataExporter>();
            foreach (var config in dataExporterConfigs)
                Exporters.Add(autofac.ResolveNamed<IDataExporter>(config.ExporterType,
                   new NamedParameter("id", config.JsonConfig)));

            ReportName = reportName;
            Query = query;
            ViewTemplate = template;
            ReportId = reportId;
            Schedule = schedule;
            this.repository = repository;
            TryCount = tryCount;
            QueryTimeOut = timeOut;
            ConnectionString = connStr;
            HasHtmlBody = Exporters
                .Any(exporter=>exporter.DataTypes.Contains("Html"));
            HasJsonAttachment = Exporters
                .Any(exporter => exporter.DataTypes.Contains("JsonBase"));
            HasXlsxAttachment = Exporters
                .Any(exporter => exporter.DataTypes.Contains("Xlsx"));
            HasTelegramView = Exporters
                .Any(exporter => exporter.DataTypes.Contains("Telegram"));
        }

        public void Execute(string address = null)
        {
            var dtoInstance = new DtoFullInstance()
            {
                StartTime = DateTime.Now,
                TaskId = Id,
                State = (int) InstanceState.InProcess
            };

            dtoInstance.Id =
                repository.CreateEntity(mapper.Map<DtoInstance>(dtoInstance));

            repository.CreateEntity(mapper.Map<DtoInstanceData>(dtoInstance));

            Stopwatch duration = new Stopwatch();
            duration.Start();
            int i = 1;
            bool dataObtained = false;

            var sendData = new SendData();

            while (!dataObtained && i <= TryCount)
            {
                try
                {
                    sendData.JsonBaseData = dataEx.Execute(this);
                    dataObtained = true;
                    i++;
                    break;
                }
                catch (Exception ex)
                {
                    sendData.JsonBaseData = ex.Message;
                }

                i++;
            }

            if (dataObtained)
            {
                if (HasHtmlBody)
                    try
                    {
                        sendData.HtmlData = viewEx.ExecuteHtml(ViewTemplate, sendData.JsonBaseData);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                        sendData.HtmlData = ex.Message;
                    }

                if (HasTelegramView)
                    try
                    {
                        sendData.TelegramData =
                            viewEx.ExecuteTelegramView(sendData.JsonBaseData, ReportName);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                        sendData.TelegramData = ex.Message;
                    }

                if (HasXlsxAttachment)
                    try
                    {
                        sendData.XlsxData = viewEx.ExecuteXlsx(sendData.JsonBaseData, ReportName);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                        sendData.XlsxData = null;
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
                    if (HasXlsxAttachment)
                        sendData.XlsxData?.Dispose();
                }

            }


            duration.Stop();
            dtoInstance.Data = archiver.CompressString(sendData.JsonBaseData);
            dtoInstance.ViewData = archiver.CompressString(sendData.HtmlData);
            dtoInstance.TryNumber = i - 1;
            dtoInstance.Duration = Convert.ToInt32(duration.ElapsedMilliseconds);
            dtoInstance.State =
                dataObtained ? (int) InstanceState.Success : (int) InstanceState.Failed;

            repository.UpdateEntity(mapper.Map<DtoInstance>(dtoInstance));
            repository.UpdateEntity(mapper.Map<DtoInstanceData>(dtoInstance));
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
                    var jsonReport = dataEx.Execute(this);
                    htmlReport = viewEx.ExecuteHtml(ViewTemplate, jsonReport);
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