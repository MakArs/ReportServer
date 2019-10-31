using Nancy.Swagger.Annotations.Attributes;
using ReportService.Interfaces.Core;

namespace ReportService.Entities.Dto
{
    [Model("Schedule")]
    public class DtoSchedule : IDtoEntity
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Schedule { get; set; }
    }
}
