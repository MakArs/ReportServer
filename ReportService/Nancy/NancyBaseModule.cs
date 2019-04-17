using System;
using System.Configuration;
using Nancy;
using ReportService.Interfaces.Core;

namespace ReportService.Nancy
{
    public class ApiTask
    {
        public int Id;
        public string Name;
        public string Parameters;
        public int? ScheduleId;
        public DtoOperation[] BindedOpers;
    }

    public class ApiOperInstance
    {
        public int Id;
        public int TaskInstanceId;
        public int OperationId;
        public DateTime StartTime;
        public int Duration;
        public int State;
        public byte[] DataSet;
        public string ErrorMessage;
        public string OperName;
    }

    public enum ApiUserRole
    {
        Viewer,
        Editor,
        StopRunner,//executor?
        NoRole
    }

    public class NancyBaseModule : NancyModule
    {
        protected string ViewPermission = ConfigurationManager.AppSettings["Permissions_View"];
        protected string EditPermission = ConfigurationManager.AppSettings["Permissions_Edit"];
        protected string StopRunPermission = ConfigurationManager.AppSettings["Permissions_StopRun"];
    }
}