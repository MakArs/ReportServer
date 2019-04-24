namespace ReportService.Operations.DataExporters.Configurations
{
    public class EmailExporterConfig : IExporterConfig
    {
        public string PackageName { get; set; }
        public bool RunIfVoidPackage { get; set; }
        public bool DateInName;
        public bool HasHtmlBody;
        public bool HasJsonAttachment;
        public bool UseAllSetsJson;
        public bool HasXlsxAttachment;
        public bool UseAllSetsXlsx;
        public string RecepientsDatasetName;
        public int RecepientGroupId;
        public string ViewTemplate;
        public string ReportName;
    }
}
