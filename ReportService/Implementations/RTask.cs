﻿using Autofac;
using ReportService.Interfaces;
using System;
using System.Diagnostics;

namespace ReportService.Implementations
{
    public enum RTaskType : byte
    {
        Common = 1,
        Custom = 2
    }

    public class RTask : IRTask
    {
        public int ID { get; }
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
        private IConfig config_;

        public RTask(ILifetimeScope aAutofac, IPostMaster aPostMaster, IConfig aConfig,
            int ID, string aTemplate, string aSchedule, string aQuery, string aSendAddress, int aTryCount,
            int aTimeOut, int TaskType)
        {
            Type = (RTaskType)TaskType;
            if (Type.ToString()== "Common")
            {
                dataEx_ = aAutofac.ResolveNamed<IDataExecutor>("commondataexx");
                viewEx_ = aAutofac.ResolveNamed<IViewExecutor>("commonviewex");
            }
            else
            {
                dataEx_ = aAutofac.ResolveNamed<IDataExecutor>(aQuery);
                viewEx_ = aAutofac.ResolveNamed<IViewExecutor>(aTemplate);
            }

            postMaster_ = aPostMaster;
            this.ID = ID;
            Query = aQuery;
            ViewTemplate = aTemplate;
            SendAddresses = aSendAddress.Split(';');
            Schedule = aSchedule;
            config_ = aConfig;
            TryCount = aTryCount;
            TimeOut = aTimeOut;
        }

        public void Execute(string aAddress = null)
        {
            int InstanceID = config_.CreateInstance(ID, "", "", 0, "InProcess", 0);
            string[] deliveryAddrs = string.IsNullOrEmpty(aAddress) ?
                SendAddresses
                : new string[] { aAddress };

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

            config_.UpdateInstance(InstanceID, jsonReport, htmlReport, duration.ElapsedMilliseconds, dataObtained ? "Success" : "Failed", i - 1);
        }
    }//class
}