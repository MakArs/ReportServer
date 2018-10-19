using System;
using Nancy;
using ReportService.Interfaces;

namespace ReportService.Nancy
{
    public class ApiTask
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int? ScheduleId { get; set; }
        public DtoOperation[] BindedOpers { get; set; }
    }

    public class ApiOperInstance
    {
        public int Id { get; set; }
        public int TaskInstanceId { get; set; }
        public int OperationId { get; set; }
        public DateTime StartTime { get; set; }
        public int Duration { get; set; }
        public int State { get; set; }
        public string DataSet { get; set; }
        public string ErrorMessage { get; set; }
        public string OperName { get; set; }
    }

    public class NancyBaseModule : NancyModule
    {
    }
}
