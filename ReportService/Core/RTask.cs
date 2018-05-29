using System;
using System.Diagnostics;
using System.Xml;
using Autofac;
using AutoMapper;
using Monik.Client;
using ReportService.Interfaces;
using Telegram.Bot;

namespace ReportService.Core
{
    public class RTask : IRTask
    {
        public int             Id                { get; }
        public RRecepientGroup SendAddresses     { get; }
        public string          ViewTemplate      { get; }
        public DtoSchedule     Schedule          { get; }
        public string          ConnectionString  { get; }
        public string          Query             { get; }
        public long            ChatId            { get; }
        public int             TryCount          { get; }
        public int             QueryTimeOut      { get; }
        public RReportType     Type              { get; }
        public int             ReportId          { get; }
        public bool            HasHtmlBody       { get; }
        public bool            HasJsonAttachment { get; }

        private readonly IDataExecutor _dataEx;
        private readonly IViewExecutor _viewEx;
        private readonly IPostMaster _postMaster;
        private readonly IRepository _repository;
        private readonly IClientControl _monik;
        private readonly IMapper _mapper;
        private readonly IArchiver _archiver;
        private readonly ITelegramBotClient _bot;

        public RTask(ILifetimeScope autofac, IPostMaster postMaster, IRepository repository,
                     IClientControl monik, IMapper mapper, IArchiver archiver, ITelegramBotClient botClient,
                     int id, string template, DtoSchedule schedule, string connStr, string query,
                     long chatId, RRecepientGroup sendAddress, int tryCount, int timeOut,
                     RReportType reportType, int reportId, bool htmlBody, bool jsonAttach)
        {
            Type = reportType;

            switch (Type)
            {
                case RReportType.Common:
                    _dataEx = autofac.ResolveNamed<IDataExecutor>("commondataex");
                    _viewEx = autofac.ResolveNamed<IViewExecutor>("commonviewex");
                    break;
                case RReportType.Custom:
                    _dataEx = autofac.ResolveNamed<IDataExecutor>(query);
                    _viewEx = autofac.ResolveNamed<IViewExecutor>(template);
                    break;
                default:
                    throw new NotImplementedException();
            }

            _archiver         = archiver;
            _postMaster       = postMaster;
            Id                = id;
            Query             = query;
            ChatId            = chatId;
            ViewTemplate      = template;
            ReportId          = reportId;
            SendAddresses     = sendAddress;
            Schedule          = schedule;
            _repository       = repository;
            TryCount          = tryCount;
            QueryTimeOut      = timeOut;
            ConnectionString  = connStr;
            HasHtmlBody       = htmlBody;
            HasJsonAttachment = jsonAttach;
            _monik            = monik;
            _mapper           = mapper;
            _bot              = botClient;
        }

        public void Execute(string address = null)
        {
            var dtoInstance = new DtoFullInstance()
            {
                StartTime = DateTime.Now,
                TaskId    = Id,
                State     = (int) InstanceState.InProcess
            };

            dtoInstance.Id =
                _repository.CreateEntity(_mapper.Map<DtoInstance>(dtoInstance));

            _repository.CreateEntity(_mapper.Map<DtoInstanceData>(dtoInstance));

            string[] deliveryAddrs;

            if (!string.IsNullOrEmpty(address))
                deliveryAddrs = new[] {address};
            else
            {
                try
                {
                    var addrArray = SendAddresses.GetAddresses();
                    deliveryAddrs = addrArray;
                }
                catch
                {
                    _monik.ApplicationWarning($"Нет списка получателей для отчёта {Id}");
                    return;
                }
            }

            Stopwatch duration = new Stopwatch();
            duration.Start();
            int    i            = 1;
            bool   dataObtained = false;
            string jsonReport   = "";
            string htmlReport   = "";

            while (!dataObtained && i <= TryCount)
            {
                try
                {
                    jsonReport   = _dataEx.Execute(this);
                    htmlReport   = _viewEx.Execute(ViewTemplate, jsonReport);
                    dataObtained = true;
                    i++;
                    break;
                }
                catch (Exception ex)
                {
                    jsonReport = ex.Message;
                    htmlReport = ex.Message;
                }

                i++;
            }

            if (dataObtained)
            {
                try
                {
                    _postMaster.Send(deliveryAddrs,
                        HasHtmlBody ? htmlReport : null,
                        HasJsonAttachment ? jsonReport : null);
                    if (ChatId!=0)
                    {
                        try
                        {

                        var xml = new XmlDocument();
                        xml.LoadXml(htmlReport);
                        _bot.SendTextMessageAsync(ChatId, xml.InnerText).Wait();
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine(e);
                            throw;
                        }
                    }
                    _monik.ApplicationInfo($"Отчёт {Id} успешно выслан");
                }
                catch (Exception e)
                {
                    _monik.ApplicationError(e.Message);
                }
            }

            duration.Stop();

            dtoInstance.Data      = _archiver.CompressString(jsonReport);
            dtoInstance.ViewData  = _archiver.CompressString(htmlReport);
            dtoInstance.TryNumber = i - 1;
            dtoInstance.Duration  = Convert.ToInt32(duration.ElapsedMilliseconds);
            dtoInstance.State     = dataObtained ? (int) InstanceState.Success : (int) InstanceState.Failed;

            // string filename = $@"{AppDomain.CurrentDomain.BaseDirectory}\\Report{Id}-{DateTime.Now:HHmmss}";

            _repository.UpdateEntity(_mapper.Map<DtoInstance>(dtoInstance));
            _repository.UpdateEntity(_mapper.Map<DtoInstanceData>(dtoInstance));
        } //method

        public string GetCurrentView()
        {
            int    i            = 1;
            bool   dataObtained = false;
            string htmlReport   = "";

            while (!dataObtained && i <= TryCount)
            {
                try
                {
                    var jsonReport = _dataEx.Execute(this);
                    htmlReport   = _viewEx.Execute(ViewTemplate, jsonReport);
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
    } //class
}
