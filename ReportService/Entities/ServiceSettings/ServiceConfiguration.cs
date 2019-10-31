namespace ReportService.Entities.ServiceSettings
{
    public class ServiceConfiguration
    {
        public string BotToken { get; set; }
        public string DBConnStr { get; set; }
        public string B2BConnStr { get; set; }
        public string AdministrativeAddresses { get; set; }
        public string ArchiveFormat { get; set; }
        public EmailSenderSettings EmailSenderSettings { get; set; }
        public MonikSettings MonikSettings { get; set; }
        public TokenValidationSettings TokenValidationSettings { get; set; }
        public PermissionsSettings PermissionsSettings { get; set; }
        public ProxySettings ProxySettings { get; set; }
    }
}
