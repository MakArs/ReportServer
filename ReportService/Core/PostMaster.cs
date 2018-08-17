using System;
using System.Configuration;
using System.IO;
using System.Net.Mail;
using Monik.Client;
using OfficeOpenXml;
using ReportService.Extensions;
using ReportService.Interfaces;

namespace ReportService.Core
{
    internal class PostMasterTest : IPostMaster
    {
        private string filename;

        public void Send(string reportName, RecepientAddresses addresses, string htmlReport = null, string jsonReport = null, ExcelPackage xlsReport = null)
        {
            filename = $"Report_{reportName}_{DateTime.Now:HHmmss}.html";

            using (FileStream fs = new FileStream($@"C:\ArsMak\job\{filename}", FileMode.CreateNew))
            {
                byte[] array =
                    System.Text.Encoding.Default.GetBytes(string.IsNullOrEmpty(htmlReport) ? "" : htmlReport);
                fs.Write(array, 0, array.Length);
            }

            Console.WriteLine($"file {filename} saved to disk...");
        }
    } //saving at disk

    public class PostMasterWork : IPostMaster
    {
        public void Send(string reportName, RecepientAddresses addresses, string htmlReport = null, string jsonReport = null, ExcelPackage xlsxReport = null)
        {
            string filename = reportName + $" {DateTime.Now:dd.MM.yy HHmmss}";

            string filenameJson = $@"{filename}.json";
            string filenameXlsx = $@"{filename}.xlsx";
            bool   hasHtml  = !string.IsNullOrEmpty(htmlReport);
            bool   hasJson  = !string.IsNullOrEmpty(jsonReport);
            bool   hasXlsx  = xlsxReport != null;

            using (var client = new SmtpClient(ConfigurationManager.AppSettings["SMTPServer"], 25))
            using (var msg = new MailMessage())
            {
                client.EnableSsl = true;
                client.DeliveryMethod = SmtpDeliveryMethod.Network;

                msg.From = new MailAddress(ConfigurationManager.AppSettings["from"]);
                msg.AddRecepients(addresses);

                msg.Subject = reportName + $" {DateTime.Now:dd.MM.yy}";

                if (hasHtml)
                {
                    msg.IsBodyHtml = true;
                    msg.Body = htmlReport;
                }


                MemoryStream streamJson = null;
                MemoryStream streamXlsx = null;

                try
                {
                    if (hasJson)
                    {
                        streamJson = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(jsonReport));
                        msg.Attachments.Add(new Attachment(streamJson, filenameJson, @"application/json"));
                    }

                    if (hasXlsx)
                    {
                        streamXlsx = new MemoryStream();
                        xlsxReport.SaveAs(streamXlsx);
                        streamXlsx.Position = 0;
                        msg.Attachments.Add(new Attachment(streamXlsx, filenameXlsx, @"application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"));
                    }

                    client.Send(msg);
                }
                finally
                {
                    streamJson?.Dispose();

                    streamXlsx?.Dispose();
                }
            }
        }
    }
}
