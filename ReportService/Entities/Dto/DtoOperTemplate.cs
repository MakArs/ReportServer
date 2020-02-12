using Dapper.Contrib.Extensions;
using ReportService.Interfaces.Core;

namespace ReportService.Entities.Dto
{
    [Table("OperTemplate")]
    public class DtoOperTemplate : IDtoEntity
    {
        [Key]
        public int Id { get; set; }
        public string ImplementationType { get; set; }
        public string Name { get; set; }
        public string ConfigTemplate { get; set; }
    }
}