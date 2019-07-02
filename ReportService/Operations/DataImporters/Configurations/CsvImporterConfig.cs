using ReportService.Interfaces.Operations;

namespace ReportService.Operations.DataImporters.Configurations
{
    public class CsvImporterConfig : IImporterConfig
    {
        public string PackageName { get; set; }
        public string FileFolder;
        public string FileName;
        public string Delimiter;
        public string DataSetName;
        public string GroupNumbers;
    }
}