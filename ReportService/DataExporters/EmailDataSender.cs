using System;
using System.Configuration;
using System.IO;
using System.Net.Mail;
using Autofac;
using AutoMapper;
using ReportService.Extensions;
using ReportService.Interfaces;

namespace ReportService.DataExporters
{
    public class EmailDataSender : CommonDataExporter
    {
        private readonly RecepientAddresses addresses;
        public bool HasHtmlBody;
        public bool HasXlsxAttachment;
        public bool HasJsonAttachment;
        public string RecepientsDatasetName;
        private readonly IViewExecutor viewExecutor;
        public string ViewTemplate;
        public string ReportName;

        public EmailDataSender(IMapper mapper, ILogic logic, ILifetimeScope autofac,
                               EmailExporterConfig config)
        {
            mapper.Map(config, this);

            addresses = config.RecepientGroupId > 0
                ? logic.GetRecepientAddressesByGroupId(config.RecepientGroupId)
                : new RecepientAddresses();

            viewExecutor = autofac.ResolveNamed<IViewExecutor>("commonviewex");
        } //ctor

        public override void Send(IRTaskRunContext taskContext)
        {
            var dataSet = taskContext.DataSets[DataSetName];

            if (!RunIfVoidDataSet && (string.IsNullOrEmpty(dataSet) || dataSet == "[]"))
                return;

            string filename = ReportName + $" {DateTime.Now:dd.MM.yy HH:mm:ss}";

            string filenameJson = $@"{filename}.json";
            string filenameXlsx = $@"{filename}.xlsx";

            using (var client = new SmtpClient(ConfigurationManager.AppSettings["SMTPServer"], 25))
            using (var msg = new MailMessage())
            {
                client.EnableSsl = true;
                client.DeliveryMethod = SmtpDeliveryMethod.Network;

                msg.From = new MailAddress(ConfigurationManager.AppSettings["from"]);
                msg.AddRecepientsFromGroup(addresses);
                if (!string.IsNullOrEmpty(RecepientsDatasetName))
                    msg.AddRecepientsFromDataSet(taskContext.DataSets[RecepientsDatasetName]);

                msg.Subject = ReportName + $" {DateTime.Now:dd.MM.yy}";

                if (HasHtmlBody)
                {
                    msg.IsBodyHtml = true;
                    msg.Body = viewExecutor.ExecuteHtml(ViewTemplate, dataSet);
                }

                MemoryStream streamJson = null;
                MemoryStream streamXlsx = null;

                try
                {
                    if (HasJsonAttachment)
                    {
                        streamJson =
                            new MemoryStream(
                                System.Text.Encoding.UTF8.GetBytes(dataSet));
                        msg.Attachments.Add(new Attachment(streamJson, filenameJson,
                            @"application/json"));
                    }

                    if (HasXlsxAttachment)
                    {
                        streamXlsx = new MemoryStream();
                        var excel = viewExecutor.ExecuteXlsx(dataSet, ReportName);
                        excel.SaveAs(streamXlsx);
                        excel.Dispose();
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
        } //method
    }
}