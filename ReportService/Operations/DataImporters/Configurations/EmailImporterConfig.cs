using ReportService.Interfaces.Operations;

namespace ReportService.Operations.DataImporters.Configurations
{
    public class EmailImporterConfig : IImporterConfig
    {
        public string PackageName { get; set; }
        public string ServerHost;
        public int Port;
        public string Email;
        public string Password;
        public string SenderEmail;
        public string AttachmentName;
        public int SearchDays;
    }
}
