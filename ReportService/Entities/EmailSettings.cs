namespace ReportService.Entities
{
    public class EmailSettings
    {
        public string ServerHost { get; set; }
        public int Port { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public bool UseImapSecureOptions { get; set; }
        public string SenderEmail { get; set; }
        public string AttachmentName { get; set; }
        public int MaxRetryCount { get; set; }
        public int SearchDays { get; set; }
    }
}