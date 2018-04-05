using System;
using Nancy;

namespace ReportService.Nancy
{
    public class ApiTask
    {
        public int Id { get; set; }
        public int ScheduleId { get; set; }
        public string ConnectionString { get; set; }
        public string ViewTemplate { get; set; }
        public string Query { get; set; }
        public int RecepientGroupId { get; set; }
        public int TryCount { get; set; }
        public int QueryTimeOut { get; set; }
        public int TaskType { get; set; }
    }

    public class ApiTaskCompact
    {
        public int Id { get; set; }
        public int ScheduleId { get; set; }
        public string ConnectionString { get; set; }
        public int RecepientGroupId { get; set; }
        public int TryCount { get; set; }
        public int QueryTimeOut { get; set; }
        public int TaskType { get; set; }
    }

    public class ApiSchedule
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Schedule { get; set; }
    }

    public class ApiRecepientGroup
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Addresses { get; set; }
    }

    public class NancyBaseModule : NancyModule
    {
        public NancyBaseModule()
        {
        }
    }
}
