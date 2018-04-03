using System;
using System.Diagnostics;
using Autofac;
using Monik.Client;
using ReportService.Interfaces;

namespace ReportService.Core
{
    public class RTask : IRTask
    {
        public int Id { get; }
        public string[] SendAddresses { get; }
        public string ViewTemplate { get; }
        public string Schedule { get; }
        public string ConnectionString { get; }
        public string Query { get; }
        public int TryCount { get; }
        public int TimeOut { get; }
        public RTaskType Type { get; }

        private readonly IDataExecutor _dataEx;
        private readonly IViewExecutor _viewEx;
        private readonly IPostMaster _postMaster;
        private readonly IRepository _repository;
        private readonly IClientControl _monik;

        public RTask(ILifetimeScope autofac, IPostMaster postMaster, IRepository repository, IClientControl monik,
            int id, string template, string schedule, string query, string sendAddress, int tryCount,
            int timeOut, RTaskType taskType, string connStr)
        {
            Type = taskType;

            switch (Type)
            {
                case RTaskType.Common:
                    _dataEx = autofac.ResolveNamed<IDataExecutor>("commondataex");
                    _viewEx = autofac.ResolveNamed<IViewExecutor>("commonviewex");
                    break;
                case RTaskType.Custom:
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
            SendAddresses = sendAddress.Split(';');
            Schedule = schedule;
            _repository = repository;
            TryCount = tryCount;
            TimeOut = timeOut;
            ConnectionString = connStr;
            _monik = monik;
        }

        public void Execute(string address = null)
        {
            var dtoInstance = new DTOInstance()
            {
                StartTime = DateTime.Now,
                TaskId = Id,
                State = (int) InstanceState.InProcess
            };

            dtoInstance.Id = _repository.CreateInstance(dtoInstance);
            string[] deliveryAddrs = string.IsNullOrEmpty(address)
                ? SendAddresses
                : new string[] {address};
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
            _repository.UpdateInstance(dtoInstance);
        }
    } //class
}
