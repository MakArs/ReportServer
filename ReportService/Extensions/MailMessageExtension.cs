using System.Net.Mail;

namespace ReportService.Extensions
{
    public class RecepientAddresses
    {
        public string[] To;
        public string[] Bcc;

        public bool HaveRecepients => To?.Length > 0;
    }

    public static class MailMessageExtension
    {
        public static void AddRecepients(this MailMessage msg, RecepientAddresses addresses)
        {
            AddAddressesToCollection(addresses.To, msg.To);
            AddAddressesToCollection(addresses.Bcc, msg.Bcc);
        }

        static void AddAddressesToCollection(string[] addresses, MailAddressCollection col)
        {
            if (addresses != null)
                foreach (var address in addresses)
                    if (!string.IsNullOrEmpty(address))
                        col.Add(new MailAddress(address));
        }
    }
}
