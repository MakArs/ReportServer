using ReportService.Interfaces.Core;

namespace ReportService.Entities
{
    public class DtoTask : IDtoEntity
    {
        public int Id;
        public string Name;
        public string Parameters;
        public int? ScheduleId;
    }
}