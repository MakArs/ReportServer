using ReportService.Interfaces.Operations;

namespace ReportService.Operations.DataExporters.Configurations
{
    public class TelegramExporterConfig : IExporterConfig
    {
        public string PackageName { get; set; }
        public bool RunIfVoidPackage { get; set; }
        public int TelegramChannelId;
        public bool UseAllSets;
        public string ReportName;
    }
}
