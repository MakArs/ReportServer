using Autofac;
using ReportService.Interfaces;
using System;
using System.Diagnostics;

namespace ReportService.Implementations
{
    public class RTask : IRTask
    {
        public int Id { get; }
        public string[] SendAddresses { get; set; }
        public string ViewTemplate { get; }
        public string Schedule { get; }
        public string Query { get; }
        public int TryCount { get; }
        public int TimeOut { get; }
        public RTaskType Type { get; }

        private IDataExecutor dataEx_;
        private IViewExecutor viewEx_;
        private IPostMaster postMaster_;
        private IRepository _repository;

        public RTask(ILifetimeScope aAutofac, IPostMaster aPostMaster, IRepository aRepository,
            int id, string aTemplate, string aSchedule, string aQuery, string aSendAddress, int aTryCount,
            int aTimeOut, RTaskType aTaskType)
        {
            Type = aTaskType;

            switch (Type)
            {
                case RTaskType.Common:
                    dataEx_ = aAutofac.ResolveNamed<IDataExecutor>("commondataex");
                    viewEx_ = aAutofac.ResolveNamed<IViewExecutor>("commonviewex");
                    break;
                case RTaskType.Custom:
                    dataEx_ = aAutofac.ResolveNamed<IDataExecutor>(aQuery);
                    viewEx_ = aAutofac.ResolveNamed<IViewExecutor>(aTemplate);
                    break;
                default:
                    throw new NotImplementedException();
            }

            postMaster_ = aPostMaster;
            Id = id;
            Query = aQuery;
            ViewTemplate = aTemplate;
            SendAddresses = aSendAddress.Split(';');
            Schedule = aSchedule;
            _repository = aRepository;
            TryCount = aTryCount;
            TimeOut = aTimeOut;
        }

        public void Execute(string address = null)
        {
            int instanceId = _repository.CreateInstance(Id, "", "", 0, "InProcess", 0);
            string[] deliveryAddrs = string.IsNullOrEmpty(address) ?
                SendAddresses
                : new string[] { address };

            Stopwatch duration = new Stopwatch();
            int i = 1;
            bool dataObtained = false;
            string jsonReport = "";
            string htmlReport = "";

            while (!dataObtained && i <= TryCount)
            {
                try
                {
                    jsonReport = dataEx_.Execute(Query, TimeOut);
                    htmlReport = viewEx_.Execute(ViewTemplate, jsonReport);
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
                    postMaster_.Send(htmlReport, addr);

            _repository.UpdateInstance(instanceId, jsonReport, htmlReport, duration.ElapsedMilliseconds, dataObtained ? "Success" : "Failed", i - 1);
        }
    }//class
}
