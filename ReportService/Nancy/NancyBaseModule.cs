using Nancy;
using ReportService.Interfaces;

namespace ReportService.Nancy
{
    public class ApiTask
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int? ScheduleId { get; set; }
        public DtoTaskOper[] BindedOpers { get; set; }
    }


    public class NancyBaseModule : NancyModule
    {
    }
}
