using Nancy.Swagger.Annotations.Attributes;
using ReportService.Interfaces.Core;

namespace ReportService.Entities
{
    [Model("Recipient group")]
    public class DtoRecepientGroup : IDtoEntity
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Addresses { get; set; }
        public string AddressesBcc { get; set; }
    }
}