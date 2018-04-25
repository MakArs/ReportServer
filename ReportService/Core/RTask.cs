using System;
using System.Diagnostics;
using System.Runtime.Remoting.Messaging;
using Autofac;
using AutoMapper;
using Monik.Client;
using ReportService.Interfaces;

namespace ReportService.Core
{
    public class RTask : IRTask
    {
        public int Id { get; }
        public RRecepientGroup SendAddresses { get; }
        public string ViewTemplate { get; }
        public RSchedule Schedule { get; }
        public string ConnectionString { get; }
        public string Query { get; }
        public int TryCount { get; }
        public int QueryTimeOut { get; }
        public RReportType Type { get; }
        public int ReportId { get; }

        private readonly IDataExecutor _dataEx;
        private readonly IViewExecutor _viewEx;
        private readonly IPostMaster _postMaster;
        private readonly IRepository _repository;
        private readonly IClientControl _monik;
        private readonly IMapper _mapper;

        public RTask(ILifetimeScope autofac, IPostMaster postMaster, IRepository repository, IClientControl monik,
            IMapper mapper,
            int id, string template, RSchedule schedule, string query, RRecepientGroup sendAddress, int tryCount,
            int timeOut, RReportType reportType, string connStr,int reportId)
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

            _postMaster = postMaster;
            Id = id;
            Query = query;
            ViewTemplate = template;
            ReportId = reportId;
            SendAddresses = sendAddress;
            Schedule = schedule;
            _repository = repository;
            TryCount = tryCount;
            QueryTimeOut = timeOut;
            ConnectionString = connStr;
            _monik = monik;
            _mapper = mapper;
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
                _repository.CreateEntity(_mapper.Map<DtoInstance>(dtoInstance));

            _repository.CreateEntity(_mapper.Map<DtoInstanceData>(dtoInstance));
            //(_mapper.Map<DtoFullInstance, DtoInstance>(dtoInstance),
            //    _mapper.Map<DtoFullInstance, DtoInstanceData>(dtoInstance));

            string[] deliveryAddrs;
            if (!string.IsNullOrEmpty(address))
                deliveryAddrs = new string[] {address};
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
            int i = 1;
            bool dataObtained = false;
            string jsonReport = "";
            string htmlReport = "";

            while (!dataObtained && i <= TryCount)
            {
                try
                {
                    jsonReport = _dataEx.Execute(this);
                    htmlReport = _viewEx.Execute(ViewTemplate, jsonReport);
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
                    _postMaster.Send(htmlReport, deliveryAddrs);
                    _monik.ApplicationInfo($"Отчёт {Id} успешно выслан");
                }
                catch (Exception e)
                {
                    _monik.ApplicationError(e.Message);
                }
            }

            duration.Stop();

            dtoInstance.Data = jsonReport;
            dtoInstance.ViewData = htmlReport;
            dtoInstance.TryNumber = i - 1;
            dtoInstance.Duration = Convert.ToInt32(duration.ElapsedMilliseconds);
            dtoInstance.State = dataObtained ? (int) InstanceState.Success : (int) InstanceState.Failed;
            _repository.UpdateEntity(_mapper.Map<DtoInstance>(dtoInstance));
            _repository.UpdateEntity(_mapper.Map<DtoInstanceData>(dtoInstance));
            
        }

        public string GetCurrentView()
        {
            int i = 1;
            bool dataObtained = false;
            string htmlReport = "";

            while (!dataObtained && i <= TryCount)
            {
                try
                {
                    var jsonReport = _dataEx.Execute(this);
                    htmlReport = _viewEx.Execute(ViewTemplate, jsonReport);
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
