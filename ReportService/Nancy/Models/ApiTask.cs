using Nancy.Swagger.Annotations.Attributes;
using ReportService.Entities;

namespace ReportService.Nancy.Models
{
    [Model("Task")]
    public class ApiTask
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Parameters { get; set; }
        public int? ScheduleId { get; set; }
        public DtoOperation[] BindedOpers { get; set; }
    }
}
