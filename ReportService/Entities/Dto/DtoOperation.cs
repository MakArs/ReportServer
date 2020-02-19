using Dapper.Contrib.Extensions;
using ReportService.Interfaces.Core;

namespace ReportService.Entities.Dto
{
    [Table(@"""Operation""")]
    public class DtoOperation : IDtoEntity
    {
        [Key]
        public long Id { get; set; }
        public long TaskId { get; set; }
        public int Number { get; set; }
        public string Name { get; set; }
        public string ImplementationType { get; set; }
        public bool IsDefault { get; set; }
        public string Config { get; set; }
        public bool IsDeleted { get; set; }
    }
}