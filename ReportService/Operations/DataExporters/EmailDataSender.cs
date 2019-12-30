using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Threading.Tasks;
using Autofac;
using AutoMapper;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using ReportService.Entities;
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
            EmailExporterConfig config, IPackageParser parser, IConfigurationRoot serviceConfig)
        {
            this.parser = parser;
            mapper.Map(config, this);
            mapper.Map(config, Properties);

            smtpServer = serviceConfig["EmailSenderSettings:SMTPServer"];
            fromAddress = serviceConfig["EmailSenderSettings:From"];

            addresses = config.RecepientGroupId > 0
                ? logic.GetRecepientAddressesByGroupId(config.RecepientGroupId)
                : new RecipientAddresses();
            this.autofac = autofac;
        } //ctor

        private SmtpClient ConfigureClient()
        {
            var client = new SmtpClient(smtpServer, 25)
            {
                DeliveryFormat = SmtpDeliveryFormat.International,
                EnableSsl = true,
                DeliveryMethod = SmtpDeliveryMethod.Network
            };

            return client;
        }

        private MailMessage ConfigureMessage(IReportTaskRunContext taskContext, string filename)
        {
            var msg = new MailMessage
            {
                From = new MailAddress(fromAddress),
                Subject = filename
            };

            msg.AddRecipientsFromGroup(addresses);

            if (!string.IsNullOrEmpty(RecepientsDatasetName))
                msg.AddRecipientsFromPackage(taskContext.Packages[RecepientsDatasetName]);

            return msg;
        }

        private void AddDataSetsJson(List<DataSetContent> dataSets, MailMessage msg,
            MemoryStream streamJson, string fileName)
        {
            msg.Attachments.Add(new Attachment(streamJson, $@"{fileName}.json",
                @"application/json"));
        }

        private void AddDataSetsXlsx(OperationPackage package, MailMessage msg,
            MemoryStream streamXlsx, string fileName)
        {
            var excel = viewExecutor.ExecuteXlsx(package, fileName, UseAllSetsXlsx);
            excel.SaveAs(streamXlsx);
            excel.Dispose();

            streamXlsx.Position = 0;

            msg.Attachments.Add(new Attachment(streamXlsx, $@"{fileName}.xlsx",
                @"application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"));
        }

        public void Execute(IReportTaskRunContext taskContext)
        {
            var package = taskContext.Packages[Properties.PackageName];

            if (!RunIfVoidPackage && package.DataSets.Count == 0)
                return;

            string fileName = (string.IsNullOrEmpty(ReportName)
                                  ? $@"{Properties.PackageName}"
                                  : taskContext.SetStringParameters(ReportName))
                              + (DateInName
                                  ? null
                                  : $" {DateTime.Now:dd.MM.yy}");

            using var client = ConfigureClient();
            using var msg = ConfigureMessage(taskContext, fileName);

            var dataSets = parser.GetPackageValues(package);
            var firstSet = dataSets.First();

            viewExecutor = firstSet.GroupColumns != null && firstSet.GroupColumns.Any()
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
                    var dataToSave = UseAllSetsJson
                        ? JsonConvert.SerializeObject(dataSets)
                        : JsonConvert.SerializeObject(dataSets.First());

                    streamJson =
                        new MemoryStream(System.Text.Encoding.UTF8
                            .GetBytes(dataToSave));

                    AddDataSetsJson(dataSets, msg, streamJson, fileName);
                }

                if (HasXlsxAttachment)
                {
                    streamXlsx = new MemoryStream();
                    AddDataSetsXlsx(package, msg, streamXlsx, fileName);
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
        } //method

        public async Task ExecuteAsync(IReportTaskRunContext taskContext)
        {
            var package = taskContext.Packages[Properties.PackageName];

            if (!RunIfVoidPackage && package.DataSets.Count == 0)
                if (!RunIfVoidPackage && package.DataSets.Count == 0)
                    return;

            string fileName = (string.IsNullOrEmpty(ReportName)
                                  ? $@"{Properties.PackageName}"
                                  : taskContext.SetStringParameters(ReportName))
                              + (DateInName
                                  ? $" {DateTime.Now:dd.MM.yy}"
                                  : null);

            using var msg = ConfigureMessage(taskContext, fileName);

            var dataSets = parser.GetPackageValues(package);
            var firstSet = dataSets.First();

            viewExecutor = firstSet.GroupColumns != null && firstSet.GroupColumns.Any()
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
                    var dataToSave = UseAllSetsJson
                        ? JsonConvert.SerializeObject(dataSets)
                        : JsonConvert.SerializeObject(dataSets.First());

                    streamJson =
                        new MemoryStream(System.Text.Encoding.UTF8
                            .GetBytes(dataToSave));

                    AddDataSetsJson(dataSets, msg, streamJson, fileName);
                }

                if (HasXlsxAttachment)
                {
                    streamXlsx = new MemoryStream();
                    AddDataSetsXlsx(package, msg, streamXlsx, fileName);
                }

                using var client = ConfigureClient();

                await using (taskContext.CancelSource.Token.Register(() => client.SendAsyncCancel()))
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

            finally
            {
                streamJson?.Dispose();

                streamXlsx?.Dispose();
            }
        }
    }
}