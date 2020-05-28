using MailKit;
using MimeKit;

namespace ReportService.Entities
{
    public class EmailMessage
    {
        public UniqueId Uid { get; set; }

        public MimeMessage Message { get; set; }
    }
}
