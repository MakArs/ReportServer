using ReportService.Interfaces;

namespace ReportService.DataExporters
{
    public interface IExporterConfig : IOperationConfig
    {
    }

    public class DbExporterConfig : IExporterConfig
    {
        public string DataSetName { get; set; }
        public string ConnectionString;
        public string TableName;
        public int DbTimeOut;
        public bool DropBefore;
    }

    public class ReportInstanceExporterConfig : IExporterConfig
    {
        public string DataSetName { get; set; }
        public string ReportName { get; set; }
        public string ConnectionString;
        public string TableName;
        public int DbTimeOut;
    }

    public class EmailExporterConfig : IExporterConfig
    {
        public string DataSetName { get; set; }
        public bool HasHtmlBody;
        public bool HasJsonAttachment;
        public bool HasXlsxAttachment;
        public int RecepientGroupId;
        public string ViewTemplate;
        public string ReportName;
    }

    public class TelegramExporterConfig : IExporterConfig
    {
        public string DataSetName { get; set; }
        public int TelegramChannelId;
        public string ReportName;
    }

}