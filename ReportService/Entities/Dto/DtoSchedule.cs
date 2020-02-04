using ReportService.Interfaces.Core;

namespace ReportService.Entities.Dto
{
    public class DtoSchedule : IDtoEntity
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Schedule { get; set; }
    }
}