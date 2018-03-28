using System;
using Nancy;

namespace ReportService.Nancy
{
    public class ApiTask
    {
        public int Id { get; set; }
        public string Schedule { get; set; }
        public string ConnectionString { get; set; }
        public string ViewTemplate { get; set; }
        public string Query { get; set; }
        public string SendAddresses { get; set; }
        public int TryCount { get; set; }
        public int QueryTimeOut { get; set; }
        public int TaskType { get; set; }
    }

    public class NancyBaseModule : NancyModule
    {
        public NancyBaseModule()
        {
        }
    }
}
