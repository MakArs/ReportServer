using Autofac;
using ReportService.Interfaces;
using System;
using System.Diagnostics;

namespace ReportService.Implementations
{
    public class RTask : IRTask
    {
        public int Id { get; }
        public string[] SendAddresses { get; }
        public string ViewTemplate { get; }
        public string Schedule { get; }
        public string Query { get; }
        public int TryCount { get; }
        public int TimeOut { get; }
        public RTaskType Type { get; }

        private readonly IDataExecutor _dataEx;
        private readonly IViewExecutor _viewEx;
        private readonly IPostMaster _postMaster;
        private readonly IRepository _repository;

        public RTask(ILifetimeScope autofac, IPostMaster postMaster, IRepository repository,
            int id, string template, string schedule, string query, string sendAddress, int tryCount,
            int timeOut, RTaskType taskType)
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
        }

        public void Execute(string address = null)
        {
            int instanceId = _repository.CreateInstance(Id, "", "", 0, "InProcess", 0);
            string[] deliveryAddrs = string.IsNullOrEmpty(address) ?
                SendAddresses
                : new string[] { address };

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
                    jsonReport = _dataEx.Execute(Query, TimeOut);
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
                foreach (string addr in deliveryAddrs)
                    _postMaster.Send(htmlReport, addr);
            duration.Stop();
            _repository.UpdateInstance(instanceId, jsonReport, htmlReport, duration.ElapsedMilliseconds, dataObtained ? "Success" : "Failed", i - 1);
        }
    }//class
}
