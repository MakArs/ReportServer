using ReportService.Interfaces.Operations;

namespace ReportService.Operations.DataImporters.Configurations
{
    public class HistoryImporterConfig : IImporterConfig
    {
        public string PackageName { get; set; }
        public long OperInstanceId;
    }
}