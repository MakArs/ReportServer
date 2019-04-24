using System;
using System.Configuration;
using Nancy;
using Nancy.Swagger.Annotations.Attributes;
using ReportService.Interfaces.Core;

namespace ReportService.Nancy.Modules
{
    [Model("Task")]
    public class ApiTask
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Parameters { get; set; }
        public int? ScheduleId { get; set; }
        public DtoOperation[] BindedOpers { get; set; }
    }

    [Model("Operation instance")]
    public class ApiOperInstance
    {
        public int Id { get; set; }
        public int TaskInstanceId { get; set; }
        public int OperationId { get; set; }
        public DateTime StartTime { get; set; }
        public int Duration { get; set; }
        public int State { get; set; }
        public byte[] DataSet { get; set; }
        public string ErrorMessage { get; set; }
        public string OperName { get; set; }
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
        protected string PermissionsType = "permissions";
        protected string ViewPermission = ConfigurationManager.AppSettings["Permissions_View"];
        protected string EditPermission = ConfigurationManager.AppSettings["Permissions_Edit"];
        protected string StopRunPermission = ConfigurationManager.AppSettings["Permissions_StopRun"];
    }
}