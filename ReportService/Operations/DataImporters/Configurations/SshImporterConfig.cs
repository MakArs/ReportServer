using ReportService.Interfaces.Operations;

namespace ReportService.Operations.DataImporters.Configurations
{
    public class SshImporterConfig : IImporterConfig
    {
        public string PackageName { get; set; }
        public string Host;
        public string Login;
        public string Password;
        public string FilePath;
    }
}