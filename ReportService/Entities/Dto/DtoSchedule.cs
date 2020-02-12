using Dapper.Contrib.Extensions;
using ReportService.Interfaces.Core;

namespace ReportService.Entities.Dto
{
    [Table("Schedule")]
    public class DtoSchedule : IDtoEntity
    {
        [Key]
        public int Id { get; set; }
        public string Name { get; set; }
        public string Schedule { get; set; }
    }
}