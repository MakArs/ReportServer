using System.Collections.Generic;
using Nancy;

namespace ReportService.Nancy
{
    public class ApiTask
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int? ScheduleId { get; set; }
        public List<int> BindedOpers { get; set; }
    }

    public class NancyBaseModule : NancyModule
    {
    }
}
