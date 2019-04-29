using ReportService.Interfaces.Core;

namespace ReportService.Entities
{
    public class DtoOperation : IDtoEntity
    {
        public int Id;
        public int TaskId;
        public int Number;
        public string Name;
        public string ImplementationType;
        public bool IsDefault;
        public string Config;
        public bool IsDeleted;
    }
}