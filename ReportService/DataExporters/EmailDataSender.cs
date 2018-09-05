using System;
using System.Configuration;
using System.IO;
using System.Net.Mail;
using Newtonsoft.Json;
using OfficeOpenXml;
using ReportService.Extensions;
using ReportService.Interfaces;

namespace ReportService.DataExporters
{
    internal class PostMasterTest : CommonDataExporter
    {
        private string filename;

        public void Send(string reportName, RecepientAddresses addresses, string htmlReport = null,
                         string jsonReport = null, ExcelPackage xlsReport = null)
        {
            filename = $"Report_{reportName}_{DateTime.Now:HHmmss}.html";

            using (FileStream fs = new FileStream($@"C:\ArsMak\job\{filename}", FileMode.CreateNew))
            {
                byte[] array =
                    System.Text.Encoding.Default.GetBytes(string.IsNullOrEmpty(htmlReport)
                        ? ""
                        : htmlReport);
                fs.Write(array, 0, array.Length);
            }

            Console.WriteLine($"file {filename} saved to disk...");
        }
    } //saving at disk

    public class EmailDataSender : CommonDataExporter
    {
        private readonly RecepientAddresses addresses;
        private readonly bool hasHtmlBody;
        private readonly bool hasXlsxAttachment;
        private readonly bool hasJsonAttachment;
        private readonly IViewExecutor viewExecutor;
        private readonly string viewTemplate;
        private readonly string reportName;

        public EmailDataSender(ILogic logic, IViewExecutor executor, string jsonConfig)
        {
            var emailConfig = JsonConvert
                .DeserializeObject<EmailExporterConfig>(jsonConfig);

            DataSetName = emailConfig.DataSetName;
            hasHtmlBody = emailConfig.HasHtmlBody;
            hasJsonAttachment = emailConfig.HasJsonAttachment;
            hasXlsxAttachment = emailConfig.HasXlsxAttachment;
            addresses = logic.GetRecepientAddressesByGroupId(emailConfig.RecepientGroupId);
            viewTemplate = emailConfig.ViewTemplate;
            viewExecutor = executor;
            reportName = emailConfig.ReportName;
        }

        public override void Send(string dataSet)
        {
            string filename = reportName + $" {DateTime.Now:dd.MM.yy HHmmss}";

            string filenameJson = $@"{filename}.json";
            string filenameXlsx = $@"{filename}.xlsx";

            using (var client = new SmtpClient(ConfigurationManager.AppSettings["SMTPServer"], 25))
            using (var msg = new MailMessage())
            {
                client.EnableSsl = true;
                client.DeliveryMethod = SmtpDeliveryMethod.Network;

                msg.From = new MailAddress(ConfigurationManager.AppSettings["from"]);
                msg.AddRecepients(addresses);

                msg.Subject = reportName + $" {DateTime.Now:dd.MM.yy}";

                if (hasHtmlBody)
                {
                    msg.IsBodyHtml = true;
                    msg.Body = viewExecutor.ExecuteHtml(viewTemplate,dataSet);
                }

                MemoryStream streamJson = null;
                MemoryStream streamXlsx = null;

                try
                {
                    if (hasJsonAttachment)
                    {
                        streamJson =
                            new MemoryStream(
                                System.Text.Encoding.UTF8.GetBytes(dataSet));
                        msg.Attachments.Add(new Attachment(streamJson, filenameJson,
                            @"application/json"));
                    }

                    if (hasXlsxAttachment)
                    {
                        streamXlsx = new MemoryStream();
                        viewExecutor.ExecuteXlsx(dataSet,reportName).SaveAs(streamXlsx);
                        streamXlsx.Position = 0;
                        msg.Attachments.Add(new Attachment(streamXlsx, filenameXlsx,
                            @"application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"));
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
