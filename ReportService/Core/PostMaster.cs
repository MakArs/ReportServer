using System;
using System.IO;
using System.Net.Mail;
using ReportService.Interfaces;
using System.Configuration;
using Monik.Client;

namespace ReportService.Implementations
{
    internal class PostMasterTest : IPostMaster
    {
        private string _filename;

        public void Send(string report, string[] addresses)
        {
            _filename = $"Report{DateTime.Now:HHmmss}.html";

            using (FileStream fs = new FileStream($@"C:\ArsMak\job\{_filename}", FileMode.CreateNew))
            {
                byte[] array = System.Text.Encoding.Default.GetBytes(report);
                fs.Write(array, 0, array.Length);
            }

            Console.WriteLine($"file {_filename} saved to disk...");
        }
    } //saving at disk

    public class PostMasterWork : IPostMaster
    {
        private readonly IClientControl _monik;

        public PostMasterWork(IClientControl monik)
        {
            _monik = monik;
        }
        public void Send(string report, string[] addresses)
        {
            SmtpClient client = new SmtpClient(ConfigurationManager.AppSettings["SMTPServer"], 25);
            MailMessage msg = new MailMessage();
            msg.From = new MailAddress(ConfigurationManager.AppSettings["from"]);
            foreach (var address in addresses)
                msg.To.Add(new MailAddress(address));
            msg.Subject = "Отчёт";
            msg.IsBodyHtml = true;
            msg.Body = report;

            client.EnableSsl = true;
            client.DeliveryMethod = SmtpDeliveryMethod.Network;

            try
            {
                client.Send(msg);
            }
            catch (Exception ex)
            {
                _monik.ApplicationError($"Отчёт не выслан: " + ex.Message);
            }
            finally
            {
                msg.Dispose();
            }
        }
    }
}
