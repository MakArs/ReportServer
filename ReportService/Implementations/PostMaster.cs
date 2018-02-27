using System;
using System.IO;
using System.Net;
using System.Net.Mail;
using ReportService.Interfaces;
using System.Configuration;

namespace ReportService.Implementations
{
    class PostMasterTest : IPostMaster
    {
        private string filename;

        public void Send(string report, string address)
        {
            filename = $"Report{DateTime.Now.ToString("HHmmss")}.html";

            using (FileStream fs = new FileStream($@"C:\ArsMak\job\{filename}", FileMode.CreateNew))
            {
                byte[] array = System.Text.Encoding.Default.GetBytes(report);
                fs.Write(array, 0, array.Length);
            }

            Console.WriteLine($"file {filename} saved to disk...");
        }
    } //saving at disk

    class PostMasterWork : IPostMaster
    {
        SmtpClient client = new SmtpClient("smtp.mail.ru", 587);

        // TODO: add subject generation,change adress
        public void Send(string report, string address)
        {
            MailMessage msg = new MailMessage();
            msg.From = new MailAddress(ConfigurationManager.AppSettings["from"]);
            msg.To.Add(new MailAddress(address));
            msg.Subject = "Testing";
            msg.IsBodyHtml = true;
            msg.Body = report;

            client.EnableSsl = true;
            client.DeliveryMethod = SmtpDeliveryMethod.Network;
            client.Credentials = new NetworkCredential(ConfigurationManager.AppSettings["from"], ConfigurationManager.AppSettings["pass"]);

            try
            {
                client.Send(msg);
                Console.WriteLine($"Mail to {address} has been successfully sent!");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Fail Has error" + ex.Message);
            }
            finally
            {
                msg.Dispose();
            }
        }
    }
}
