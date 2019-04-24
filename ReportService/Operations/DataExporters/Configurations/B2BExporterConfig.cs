namespace ReportService.Operations.DataExporters.Configurations
{
    public class B2BExporterConfig : IExporterConfig
    {
        public string PackageName { get; set; }
        public bool RunIfVoidPackage { get; set; }
        public string ReportName;
        public string Description;
        public string ConnectionString;
        public string ExportTableName;
        public string ExportInstanceTableName;
        public int DbTimeOut;
    }
}
