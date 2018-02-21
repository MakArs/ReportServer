using System;
using System.IO;
using System.Net;
using System.Net.Mail;
using ReportService.Interfaces;
using System.Configuration;
using System.Collections.Specialized;

namespace ReportService.Implementations
{
    class PostMasterTest : IPostMaster
    {
        private string filename;

        public void Send(string report,string address)
        {
            filename = $"Report{DateTime.Now.ToString("HHmmss")}.html";

            using (FileStream fs = new FileStream($@"C:\ArsMak\job\{filename}", FileMode.CreateNew))
            {
                byte[] array = System.Text.Encoding.Default.GetBytes(report);
                fs.Write(array, 0, array.Length);
            }

            Console.WriteLine($"file {filename} saved to disk...");
        }
    }

    class PostMasterWork : IPostMaster
    {
        MailMessage msg = new MailMessage();
        SmtpClient client = new SmtpClient("smtp.mail.ru", 587);

        public void Send(string report, string address)
        {
            msg.From = new MailAddress(ConfigurationManager.AppSettings["from"]);
            msg.To.Add(new MailAddress(address));
            msg.Subject = "Testing";
            msg.IsBodyHtml = true;
            msg.Body= report;
            client.EnableSsl = true;
            client.DeliveryMethod = SmtpDeliveryMethod.Network;
            client.Credentials = new NetworkCredential(ConfigurationManager.AppSettings["from"], ConfigurationManager.AppSettings["pass"]);
            try
            {
                client.Send(msg);
                Console.WriteLine( "Mail has been successfully sent!");
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
