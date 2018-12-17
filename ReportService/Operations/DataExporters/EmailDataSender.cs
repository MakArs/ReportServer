using System;
using System.Configuration;
using System.IO;
using System.Net.Mail;
using System.Threading.Tasks;
using Autofac;
using AutoMapper;
using Newtonsoft.Json;
using ReportService.Extensions;
using ReportService.Interfaces.Core;
using ReportService.Interfaces.ReportTask;

namespace ReportService.Operations.DataExporters
{
    public class EmailDataSender : IOperation
    {
        public CommonOperationProperties Properties { get; set; } = new CommonOperationProperties();
        public bool RunIfVoidPackage { get; set; }

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
            mapper.Map(config, Properties);

            addresses = config.RecepientGroupId > 0
                ? logic.GetRecepientAddressesByGroupId(config.RecepientGroupId)
                : new RecepientAddresses();

            viewExecutor = autofac.ResolveNamed<IViewExecutor>("commonviewex");
        } //ctor


        public void Execute(IRTaskRunContext taskContext)
        {
            var package = taskContext.Packages[Properties.PackageName];

            if (!RunIfVoidPackage && package.DataSets.Count == 0)
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
                    msg.AddRecepientsFromPackage(taskContext.Packages[RecepientsDatasetName]);

                msg.Subject = ReportName + $" {DateTime.Now:dd.MM.yy}";

                if (HasHtmlBody)
                {
                    msg.IsBodyHtml = true;
                    msg.Body = viewExecutor.ExecuteHtml(ViewTemplate, package);
                }

                MemoryStream streamJson = null;
                MemoryStream streamXlsx = null;

                try
                {
                    if (HasJsonAttachment)
                    {
                        streamJson =
                            new MemoryStream(System.Text.Encoding.UTF8
                                .GetBytes(JsonConvert.SerializeObject(package)));
                        msg.Attachments.Add(new Attachment(streamJson, filenameJson,
                            @"application/json"));
                    }

                    if (HasXlsxAttachment)
                    {
                        streamXlsx = new MemoryStream();
                        var excel = viewExecutor.ExecuteXlsx(package, ReportName);
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

        public async Task ExecuteAsync(IRTaskRunContext taskContext)
        {
           // using (taskContext.CancelSource.Token.Register(() =>
             //   throw new NotImplementedException("Operation was canceled3312")))
         //   {
                var package = taskContext.Packages[Properties.PackageName];
                
                if (!RunIfVoidPackage && package.DataSets.Count == 0)
                    return;

                string filename = ReportName + $" {DateTime.Now:dd.MM.yy HH:mm:ss}";

                string filenameJson = $@"{filename}.json";
                string filenameXlsx = $@"{filename}.xlsx";

                using (var msg = new MailMessage())
                {
                    msg.From = new MailAddress(ConfigurationManager.AppSettings["from"]);
                    msg.AddRecepientsFromGroup(addresses);

                    if (!string.IsNullOrEmpty(RecepientsDatasetName))
                        msg.AddRecepientsFromPackage(taskContext.Packages[RecepientsDatasetName]);

                    msg.Subject = ReportName + $" {DateTime.Now:dd.MM.yy}";

                    if (HasHtmlBody)
                    {
                        msg.IsBodyHtml = true;
                        msg.Body = viewExecutor.ExecuteHtml(ViewTemplate, package);
                    }

                    MemoryStream streamJson = null;
                    MemoryStream streamXlsx = null;

                    try
                    {
                        if (HasJsonAttachment)
                        {
                            streamJson =
                                new MemoryStream(System.Text.Encoding.UTF8
                                    .GetBytes(JsonConvert.SerializeObject(package)));
                            msg.Attachments.Add(new Attachment(streamJson, filenameJson,
                                @"application/json"));
                        }

                        if (HasXlsxAttachment)
                        {
                            streamXlsx = new MemoryStream();
                            var excel = viewExecutor.ExecuteXlsx(package, ReportName);
                            excel.SaveAs(streamXlsx);
                            excel.Dispose();
                            streamXlsx.Position = 0;
                            msg.Attachments.Add(new Attachment(streamXlsx, filenameXlsx,
                                @"application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"));
                        }

                        using (var client = new SmtpClient(ConfigurationManager.AppSettings["SMTPServer"], 25))
                        {
                            client.EnableSsl = true;
                            client.DeliveryMethod = SmtpDeliveryMethod.Network;
                            await Task.Delay(20000);
                            using (taskContext.CancelSource.Token.Register(() => client.SendAsyncCancel()))
                                await client.SendMailAsync(msg);
                        }
                    }
                    finally
                    {
                        streamJson?.Dispose();

                        streamXlsx?.Dispose();
                    }
                }
           // }
        }
    }
}