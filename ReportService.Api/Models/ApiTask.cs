using ReportService.Entities;
using ReportService.Entities.Dto;

namespace ReportService.Api.Models
{
    //[Model("Task")]
    public class ApiTask
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public string Parameters { get; set; }
        public TaskDependency[] DependsOn { get; set; }
        public int? ScheduleId { get; set; }
        public DtoOperation[] BindedOpers { get; set; }
    }
}
