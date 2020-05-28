using Autofac;
using AutoMapper;
using ReportService.Entities;
using ReportService.Interfaces.Core;
using ReportService.Interfaces.Operations;
using ReportService.Interfaces.ReportTask;
using ReportService.Operations.DataImporters.Configurations;
using System.IO;
using System.Threading.Tasks;

namespace ReportService.Operations.DataImporters
{
    class EmailAttachementImporter : IOperation
    {
        public bool CreateDataFolder { get; set; }
        public CommonOperationProperties Properties { get; set; } = new CommonOperationProperties();
        private readonly ILifetimeScope autofac;
        public EmailSettings emailSettings;

        public EmailAttachementImporter(IMapper mapper, ILifetimeScope autofac, EmailImporterConfig config)
        {
            CreateDataFolder = true;

            emailSettings = new EmailSettings { UseImapSecureOptions = true };

            mapper.Map(config, emailSettings);

            this.autofac = autofac;
        }

        public async Task ExecuteAsync(IReportTaskRunContext taskContext)
        {
            var service = autofac.Resolve<IEmailClientService>();

            var attachmentMimePart = await service.GetFileFromEmail
                (emailSettings, taskContext.CancelSource.Token);

            using FileStream fstr =
               File.Create(Path.Combine(taskContext.DataFolderPath,
                  emailSettings.AttachmentName));

            attachmentMimePart.Content.DecodeTo(fstr);
        }
    }
}
