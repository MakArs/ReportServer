using ReportService.Interfaces.Core;

namespace ReportService.Entities.Dto
{
    public class DtoOperation : IDtoEntity
    {
        public long Id;
        public long TaskId;
        public int Number;
        public string Name;
        public string ImplementationType;
        public bool IsDefault;
        public string Config;
        public bool IsDeleted;
    }
}