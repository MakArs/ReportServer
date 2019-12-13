//using Nancy.Swagger.Annotations.Attributes;
using ReportService.Interfaces.Core;

namespace ReportService.Entities.Dto
{
    //[Model("Recipient group")] //todo:kick Nancy
    public class DtoRecepientGroup : IDtoEntity
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Addresses { get; set; }
        public string AddressesBcc { get; set; }
    }
}