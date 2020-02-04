using ReportService.Interfaces.Operations;

namespace ReportService.Operations.DataExporters.Configurations
{
    public class DbExporterConfig : IExporterConfig
    {
        public string PackageName { get; set; }
        public bool RunIfVoidPackage { get; set; }
        public string ConnectionString;
        public string TableName;
        public int DbTimeOut;
        public bool DropBefore;
        public bool CreateTable;
    }
}