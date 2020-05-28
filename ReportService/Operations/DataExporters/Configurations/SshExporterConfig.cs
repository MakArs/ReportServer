using ReportService.Interfaces.Operations;

namespace ReportService.Operations.DataExporters.Configurations
{
    public class SshExporterConfig : IExporterConfig
    {
        public string PackageName { get; set; }
        public bool RunIfVoidPackage { get; set; }
        public bool DateInName;
        public string SourceFileFolder;
        public string FileName;
        public bool ConvertPackageToXlsx;
        public bool ConvertPackageToJson;
        public bool ConvertPackageToCsv;
        public bool ConvertPackageToXml;
        public bool UseAllSets;
        public string PackageRename;
        public string Host;
        public string Login;
        public string Password;
        public string FolderPath;
        public int ClearInterval;
    }
}