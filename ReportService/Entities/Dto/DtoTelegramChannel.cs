using ReportService.Interfaces.Core;

namespace ReportService.Entities.Dto
{
    public class DtoTelegramChannel : IDtoEntity
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public long ChatId { get; set; }
        public int Type { get; set; } //from nuget types enum
    }
}