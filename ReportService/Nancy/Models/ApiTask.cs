using Nancy.Swagger.Annotations.Attributes;
using ReportService.Entities;
using ReportService.ReportTask;

namespace ReportService.Nancy.Models
{
    [Model("Task")]
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
