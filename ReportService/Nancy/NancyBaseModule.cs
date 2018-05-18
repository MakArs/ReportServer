using Nancy;

namespace ReportService.Nancy
{
    public class ApiFullTask
    {
        public int Id { get; set; }
        public int ReportId { get; set; }
        public int ScheduleId { get; set; }
        public int RecepientGroupId { get; set; }
        public int TryCount { get; set; }
        public string ConnectionString { get; set; }
        public string ViewTemplate { get; set; }
        public string Query { get; set; }
        public int QueryTimeOut { get; set; }
        public int ReportType { get; set; }
        public bool HasHtmlBody { get; set; }
        public bool HasJsonAttachment { get; set; }
    }

    public class ApiTask
    {
        public int Id { get; set; }
        public int ReportId { get; set; }
        public int ScheduleId { get; set; }
        public int RecepientGroupId { get; set; }
        public int TryCount { get; set; }
        public int QueryTimeOut { get; set; }
        public int ReportType { get; set; }
        public bool HasHtmlBody { get; set; }
        public bool HasJsonAttachment { get; set; }
    }

    public class NancyBaseModule : NancyModule
    {
    }
}
