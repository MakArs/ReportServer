using ReportService.Interfaces.Core;

namespace ReportService.Entities.Dto
{
    public class DtoRecepientGroup : IDtoEntity
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Addresses { get; set; }
        public string AddressesBcc { get; set; }
    }
}