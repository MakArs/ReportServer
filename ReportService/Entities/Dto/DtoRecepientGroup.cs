using Dapper.Contrib.Extensions;
using ReportService.Interfaces.Core;

namespace ReportService.Entities.Dto
{
    [Table(@"""RecepientGroup""")]
    public class DtoRecepientGroup : IDtoEntity //todo ArsMak: carefully fix typo everywhere
    {
        [Key]
        public int Id { get; set; }
        public string Name { get; set; }
        public string Addresses { get; set; }
        public string AddressesBcc { get; set; }
    }
}
