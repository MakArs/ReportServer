using System.IO;
using System.Threading.Tasks;
using AutoMapper;
using Renci.SshNet;
using Renci.SshNet.Common;
using ReportService.Entities;
using ReportService.Interfaces.Operations;
using ReportService.Interfaces.ReportTask;
using ReportService.Operations.DataImporters.Configurations;

namespace ReportService.Operations.DataImporters
{
    public class SshImporter : IOperation
    {
        public CommonOperationProperties Properties { get; set; } = new CommonOperationProperties();
        public string Host;
        public string Login; 
        public string Password; 
        public string FilePath; 

        public SshImporter(IMapper mapper, SshImporterConfig config)
        {
            mapper.Map(config, this);
            mapper.Map(config, Properties);
        }

        public void Execute(IReportTaskRunContext taskContext)
        {
            using (var client = new SftpClient(Host, Login, Password))
            {
                client.Connect();
                using (FileStream fstr =
                    File.Create(Path.Combine(taskContext.DataFolderPath, 
                        Path.GetFileName(FilePath) ?? throw new SftpPathNotFoundException("Incorrect file name in file path!"))))
                    client.DownloadFile(FilePath, fstr);
            }
        }

        public Task ExecuteAsync(IReportTaskRunContext taskContext)
        {
            Execute(taskContext);
            return Task.CompletedTask;
        }
    }
}