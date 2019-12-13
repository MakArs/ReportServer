using ReportService.Interfaces.Core;

namespace ReportService.Entities.Dto
{
    public class DtoTask : IDtoEntity
    {
        public long Id;
        public string Name;
        public string Parameters;
        public string DependsOn;
        public int? ScheduleId;
    }
}