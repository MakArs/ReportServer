using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Autofac;
using AutoMapper;
using Newtonsoft.Json;
using Renci.SshNet;
using ReportService.Interfaces.Core;
using ReportService.Interfaces.Operations;
using ReportService.Interfaces.Protobuf;
using ReportService.Interfaces.ReportTask;
using ReportService.Operations.DataExporters.Configurations;

namespace ReportService.Operations.DataExporters
{
    public class SshExporter : IOperation
    {
        public CommonOperationProperties Properties { get; set; } = new CommonOperationProperties();
        private readonly IViewExecutor viewExecutor;
        private readonly ILifetimeScope autofac;

        public bool DateInName;
        public string FileFolder;
        public string FileName;
        public bool RunIfVoidPackage { get; set; }
        public bool ConvertPackageToXlsx;
        public bool ConvertPackageToJson;
        public bool ConvertPackageToCsv;
        public bool ConvertPackageToXml;
        public bool UseAllSets;
        public string PackageRename;
        public string Host;
        public string Login;
        public string Password;
        public string FolderPath;
        public int ClearInterval;

        public SshExporter(IMapper mapper, ILifetimeScope autofac, SshExporterConfig config)
        {
            mapper.Map(config, this);
            mapper.Map(config, Properties);
            this.autofac = autofac;
            viewExecutor = autofac.ResolveNamed<IViewExecutor>("commonviewex");
        }

        public void Execute(IRTaskRunContext taskContext)
        {
            var package = taskContext.Packages[Properties.PackageName];

            if (!RunIfVoidPackage && package.DataSets.Count == 0)
                return;

            using (var client = new SftpClient(Host, Login, Password))
            {
                client.Connect();

                if (ClearInterval > 0)
                    CleanupFolder(client);

                if (!string.IsNullOrEmpty(FileName))
                    SaveFileToServer(taskContext, client);

                var fileName = (string.IsNullOrEmpty(PackageRename)
                                   ? $@"{Properties.PackageName}"
                                   : taskContext.SetStringParameters(PackageRename))
                               + (DateInName
                                   ? $" {DateTime.Now:dd.MM.yy}"
                                   : null);

                if (ConvertPackageToXlsx)
                    SaveXlsxPackageToServer(package,fileName, client);

                if (ConvertPackageToJson)
                    SaveJsonPackageToServer(package, fileName, client);

                if (ConvertPackageToCsv)
                    SaveCsvPackageToServer(package, fileName, client);
            }
        }

        private void CleanupFolder(SftpClient client)
        {
            var times = client.ListDirectory(FolderPath).ToList();
            var cutOff = DateTime.Now.AddDays(-ClearInterval);
            var oldfiles = times.Where(file => file.LastWriteTime < cutOff)
                .ToList();
            foreach (var file in oldfiles)
                client.DeleteFile(file.FullName);
        }

        private void SaveXlsxPackageToServer(OperationPackage package,string fileName, SftpClient client)
        {
            var filenameXlsx =fileName
                               + ".xlsx";

            using (var excel = viewExecutor.ExecuteXlsx(package, Properties.PackageName,UseAllSets))
            {
                using (var streamXlsx =
                    new MemoryStream())
                {
                    excel.SaveAs(streamXlsx);
                    streamXlsx.Position = 0;
                    client.UploadFile(streamXlsx, Path.Combine(FolderPath, filenameXlsx));
                }
            }
        }

        private void SaveCsvPackageToServer(OperationPackage package,string fileName, SftpClient client)
        {
            var filenameCsv = fileName
                              + ".csv";
            var csvBytes = viewExecutor.ExecuteCsv(package,useAllSets:UseAllSets);
            using (var csvStream = new MemoryStream(csvBytes))
                client.UploadFile(csvStream, Path.Combine(FolderPath, filenameCsv));
        }

        private void SaveJsonPackageToServer(OperationPackage package, string fileName, SftpClient client)
        {
            var filenameJson = fileName
                               + ".json";

            var builder = autofac.Resolve<IPackageBuilder>();

            var sets = builder.GetPackageValues(package);

            var dataToSave = UseAllSets
                ? JsonConvert.SerializeObject(sets)
                : JsonConvert.SerializeObject(sets.First());
            
            using (var streamJson = new MemoryStream(System.Text.Encoding.UTF8
                .GetBytes(dataToSave)))
            {
                client.UploadFile(streamJson, Path.Combine(FolderPath, filenameJson));
            }
        }

        private void SaveFileToServer(IRTaskRunContext taskContext, SftpClient client)
        {
            var fullPath = Path.Combine(FileFolder == "Default folder" ? taskContext.DataFolderPath : FileFolder,
                FileName);

            using (FileStream fstr =
                File.OpenRead(fullPath))
            {
                client.UploadFile(fstr, Path.Combine(FolderPath, FileName));
            }
        }

        public Task ExecuteAsync(IRTaskRunContext taskContext)
        {
            Execute(taskContext);
            return Task.CompletedTask;
        }
    }
}