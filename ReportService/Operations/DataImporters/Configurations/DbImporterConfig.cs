using ReportService.Interfaces.Operations;

namespace ReportService.Operations.DataImporters.Configurations
{
    public class DbImporterConfig : IImporterConfig
    {
        public string PackageName { get; set; }
        public string ConnectionString;
        public string Query;
        public int TimeOut;
        public string DataSetNames;
        public string GroupNumbers;
    }
}