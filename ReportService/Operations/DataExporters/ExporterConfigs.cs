using ReportService.Interfaces.Core;

namespace ReportService.Operations.DataExporters
{
    public interface IExporterConfig : IOperationConfig
    {
        bool RunIfVoidPackage { get; set; }
    }

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

    public class EmailExporterConfig : IExporterConfig
    {
        public string PackageName { get; set; }
        public bool RunIfVoidPackage { get; set; }
        public bool HasHtmlBody;
        public bool HasJsonAttachment;
        public bool HasXlsxAttachment;
        public string RecepientsDatasetName;
        public int RecepientGroupId;
        public string ViewTemplate;
        public string ReportName;
    }

    public class TelegramExporterConfig : IExporterConfig
    {
        public string PackageName { get; set; }
        public bool RunIfVoidPackage { get; set; }
        public int TelegramChannelId;
        public string ReportName;
    }

    public class SshExporterConfig : IExporterConfig
    {
        public string PackageName { get; set; }
        public bool RunIfVoidPackage { get; set; }
        public bool ConvertPackageToXlsx;
        public bool ConvertPackageToJson;
        public bool ConvertPackageToCsv;
        public bool ConvertPackageToXml;
        public string Host;
        public string Login;
        public string Password;
        public string FolderPath;
    }

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