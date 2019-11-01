using System;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Threading.Tasks;
using Autofac;
using AutoMapper;
using Newtonsoft.Json;
using ReportService.Entities;
using ReportService.Entities.ServiceSettings;
using ReportService.Extensions;
using ReportService.Interfaces.Core;
using ReportService.Interfaces.Operations;
using ReportService.Interfaces.Protobuf;
using ReportService.Interfaces.ReportTask;
using ReportService.Operations.DataExporters.Configurations;

namespace ReportService.Operations.DataExporters
{
    public class EmailDataSender : IOperation
    {
        public CommonOperationProperties Properties { get; set; } = new CommonOperationProperties();
        public bool RunIfVoidPackage { get; set; }

        private readonly RecipientAddresses addresses;
        public bool DateInName;
        public bool HasHtmlBody;
        public bool HasXlsxAttachment;
        public bool HasJsonAttachment;
        public string RecepientsDatasetName;
        private IViewExecutor viewExecutor;
        public string ViewTemplate;
        public string ReportName;
        public bool UseAllSetsJson;
        public bool UseAllSetsXlsx;
        private readonly IPackageParser parser;
        private readonly ILifetimeScope autofac;
        private readonly string smtpServer;
        private readonly string fromAddress;

        public EmailDataSender(IMapper mapper, ILogic logic, ILifetimeScope autofac,
            EmailExporterConfig config, IPackageParser parser, ServiceConfiguration serviceConfig)
        {
            this.parser = parser;
            mapper.Map(config, this);
            mapper.Map(config, Properties);

            smtpServer = serviceConfig.EmailSenderSettings.SMTPServer;
            fromAddress = serviceConfig.EmailSenderSettings.From;

            addresses = config.RecepientGroupId > 0
                ? logic.GetRecepientAddressesByGroupId(config.RecepientGroupId)
                : new RecipientAddresses();
            this.autofac = autofac;
        } //ctor

        public void Execute(IReportTaskRunContext taskContext)
        {
            var package = taskContext.Packages[Properties.PackageName];

            if (!RunIfVoidPackage && package.DataSets.Count == 0)
                return;

            string filename = (string.IsNullOrEmpty(ReportName)
                                  ? $@"{Properties.PackageName}"
                                  : taskContext.SetStringParameters(ReportName))
                              + (DateInName
                                  ? null
                                  : $" {DateTime.Now:dd.MM.yy}");

            string filenameJson = $@"{filename}.json";
            string filenameXlsx = $@"{filename}.xlsx";

            using (var client = new SmtpClient(smtpServer, 25))
            using (var msg = new MailMessage())
            {
                client.DeliveryFormat = SmtpDeliveryFormat.International;
                client.EnableSsl = true;
                client.DeliveryMethod = SmtpDeliveryMethod.Network;

                msg.From = new MailAddress(fromAddress);
                msg.AddRecipientsFromGroup(addresses);

                if (!string.IsNullOrEmpty(RecepientsDatasetName))
                    msg.AddRecipientsFromPackage(taskContext.Packages[RecepientsDatasetName]);

                msg.Subject = filename;

                var dataset = parser.GetPackageValues(package).First();

                viewExecutor = dataset.GroupColumns != null && dataset.GroupColumns.Any()
                    ? autofac.ResolveNamed<IViewExecutor>("GroupedViewex")
                    : !string.IsNullOrEmpty(ViewTemplate)
                        ? autofac.ResolveNamed<IViewExecutor>("CommonTableViewEx")
                        : autofac.ResolveNamed<IViewExecutor>("commonviewex");

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
                        var sets = parser.GetPackageValues(package);
                        var dataToSave = UseAllSetsJson
                            ? JsonConvert.SerializeObject(sets)
                            : JsonConvert.SerializeObject(sets.First());

                        streamJson =
                            new MemoryStream(System.Text.Encoding.UTF8
                                .GetBytes(dataToSave));
                        msg.Attachments.Add(new Attachment(streamJson, filenameJson,
                            @"application/json"));
                    }

                    if (HasXlsxAttachment)
                    {
                        streamXlsx = new MemoryStream();
                        var excel = viewExecutor.ExecuteXlsx(package, filename, UseAllSetsXlsx);
                        excel.SaveAs(streamXlsx);
                        excel.Dispose();
                        streamXlsx.Position = 0;
                        msg.Attachments.Add(new Attachment(streamXlsx, filenameXlsx,
                            @"application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"));
                    }

                    var tryCount = 0;
                    while (tryCount < 3)
                    {
                        try
                        {
                            client.Send(msg);
                            break;
                        }
                        catch (Exception exc)
                        {
                            if (tryCount == 2)
                                throw new Exception("Message not sent", exc);
                            else
                                tryCount++;
                        }
                    }
                }

                finally
                {
                    streamJson?.Dispose();

                    streamXlsx?.Dispose();
                }
            }
        } //method

        public async Task ExecuteAsync(IReportTaskRunContext taskContext)
        {
            var package = taskContext.Packages[Properties.PackageName];

            if (!RunIfVoidPackage && package.DataSets.Count == 0)
                if (!RunIfVoidPackage && package.DataSets.Count == 0)
                    return;

            string filename = (string.IsNullOrEmpty(ReportName)
                                  ? $@"{Properties.PackageName}"
                                  : taskContext.SetStringParameters(ReportName))
                              + (DateInName
                                  ? $" {DateTime.Now:dd.MM.yy}"
                                  : null);

            string filenameJson = $@"{filename}.json";
            string filenameXlsx = $@"{filename}.xlsx";

            using (var msg = new MailMessage())
            {
                msg.From = new MailAddress(fromAddress);
                msg.AddRecipientsFromGroup(addresses);

                if (!string.IsNullOrEmpty(RecepientsDatasetName))
                    msg.AddRecipientsFromPackage(taskContext.Packages[RecepientsDatasetName]);

                msg.Subject = filename;

                var dataset = parser.GetPackageValues(package).First();

                viewExecutor = dataset.GroupColumns != null && dataset.GroupColumns.Any()
                    ? autofac.ResolveNamed<IViewExecutor>("GroupedViewex")
                    : string.IsNullOrEmpty(ViewTemplate)
                        ? autofac.ResolveNamed<IViewExecutor>("CommonTableViewEx")
                        : autofac.ResolveNamed<IViewExecutor>("commonviewex");

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
                        var sets = parser.GetPackageValues(package);
                        var dataToSave = UseAllSetsJson
                            ? JsonConvert.SerializeObject(sets)
                            : JsonConvert.SerializeObject(sets.First());

                        streamJson =
                            new MemoryStream(System.Text.Encoding.UTF8
                                .GetBytes(dataToSave));
                        msg.Attachments.Add(new Attachment(streamJson, filenameJson,
                            @"application/json"));
                    }

                    if (HasXlsxAttachment)
                    {
                        streamXlsx = new MemoryStream();
                        var excel = viewExecutor.ExecuteXlsx(package, filename, UseAllSetsXlsx);
                        excel.SaveAs(streamXlsx);
                        excel.Dispose();
                        streamXlsx.Position = 0;
                        msg.Attachments.Add(new Attachment(streamXlsx, filenameXlsx,
                            @"application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"));
                    }

                    using (var client = new SmtpClient(smtpServer, 25))
                    {
                        client.DeliveryFormat = SmtpDeliveryFormat.International;
                        client.EnableSsl = true;
                        client.DeliveryMethod = SmtpDeliveryMethod.Network;

                        using (taskContext.CancelSource.Token.Register(() => client.SendAsyncCancel()))
                        {
                            var tryCount = 0;
                            while (tryCount < 3)
                            {
                                try
                                {
                                    await client.SendMailAsync(msg);
                                    break;
                                }
                                catch (Exception exc)
                                {
                                    if (tryCount == 2)
                                        throw new Exception("Message not sent", exc);
                                    else
                                        tryCount++;
                                }
                            }
                        }
                    }
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