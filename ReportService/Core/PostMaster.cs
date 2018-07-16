using System;
using System.Configuration;
using System.IO;
using System.Net.Mail;
using Monik.Client;
using ReportService.Interfaces;

namespace ReportService.Core
{
    internal class PostMasterTest : IPostMaster
    {
        private string _filename;

        public void Send(string reportName, string[] addresses, string htmlReport = null, string jsonReport = null)
        {
            _filename = $"Report_{reportName}_{DateTime.Now:HHmmss}.html";

            using (FileStream fs = new FileStream($@"C:\ArsMak\job\{_filename}", FileMode.CreateNew))
            {
                byte[] array =
                    System.Text.Encoding.Default.GetBytes(string.IsNullOrEmpty(htmlReport) ? "" : htmlReport);
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

        public void Send(string reportName, string[] addresses, string htmlReport = null, string jsonReport = null)
        {
            string filename = "";
            bool   hasHtml  = !string.IsNullOrEmpty(htmlReport);
            bool   hasJson  = !string.IsNullOrEmpty(jsonReport);

            SmtpClient  client = new SmtpClient(ConfigurationManager.AppSettings["SMTPServer"], 25);
            MailMessage msg    = new MailMessage();
            msg.From = new MailAddress(ConfigurationManager.AppSettings["from"]);
            foreach (var address in addresses)
                msg.To.Add(new MailAddress(address));
            msg.Subject = reportName + $" {DateTime.Now:dd.MM.yy}";

            if (hasHtml)
            {
                msg.IsBodyHtml = true;
                msg.Body       = htmlReport;
            }

            if (hasJson)
            {
                Random rand = new Random();
                filename = $@"{AppDomain.CurrentDomain.BaseDirectory}\\Report{rand.Next(32767)}.json";
                using (FileStream fstr = new FileStream(filename, FileMode.Create))
                {
                    byte[] bytePage = System.Text.Encoding.UTF8.GetBytes(jsonReport);
                    fstr.Write(bytePage, 0, bytePage.Length);
                }

                msg.Attachments.Add(new Attachment(filename));
            }

            client.EnableSsl      = true;
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

            if (hasJson)
                File.Delete(filename);
        }
    }
}
