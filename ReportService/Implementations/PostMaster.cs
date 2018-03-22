using System;
using System.IO;
using System.Net.Mail;
using ReportService.Interfaces;
using System.Configuration;

namespace ReportService.Implementations
{
    class PostMasterTest : IPostMaster
    {
        private string _filename;

        public void Send(string report, string address)
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

    class PostMasterWork : IPostMaster
    {

        public void Send(string report, string address)
        {
            SmtpClient client = new SmtpClient(ConfigurationManager.AppSettings["SMTPServer"], 25);
            MailMessage msg = new MailMessage();
            msg.From = new MailAddress(ConfigurationManager.AppSettings["from"]);
            msg.To.Add(new MailAddress(address));
            msg.Subject = "Отчёт";
            msg.IsBodyHtml = true;
            msg.Body = report;

            client.EnableSsl = true;
            client.DeliveryMethod = SmtpDeliveryMethod.Network;

            try
            {
                client.Send(msg);
                Console.WriteLine($"Mail to {address} has been successfully sent!");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error:" + ex.Message);
            }
            finally
            {
                msg.Dispose();
            }
        }
    }
}
