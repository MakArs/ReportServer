using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Autofac;
using AutoMapper;
using Newtonsoft.Json;
using ReportService.Entities;
using ReportService.Interfaces.Core;
using ReportService.Interfaces.Operations;
using ReportService.Interfaces.Protobuf;
using ReportService.Interfaces.ReportTask;
using ReportService.Operations.DataExporters.Configurations;
using WinSCP;

namespace ReportService.Operations.DataExporters
{
    public class FtpExporter : IOperation
    {
        public CommonOperationProperties Properties { get; set; } = new CommonOperationProperties();
        private readonly IViewExecutor viewExecutor;
        private readonly ILifetimeScope autofac;

        public bool DateInName;
        public string SourceFileFolder;
        public string FileName;
        public bool RunIfVoidPackage { get; set; }
        public bool ConvertPackageToXlsx;
        public bool ConvertPackageToJson;

        public bool ConvertPackageToCsv;

        //public bool ConvertPackageToXml; //in future
        public bool UseAllSets;
        public string PackageRename;
        public string Host;
        public string Login;
        public string Password;

        public string FolderPath;
        public int ClearInterval;

        public FtpExporter(IMapper mapper, ILifetimeScope autofac, FtpExporterConfig config)
        {
            mapper.Map(config, this);
            mapper.Map(config, Properties);
            this.autofac = autofac;
            viewExecutor = autofac.ResolveNamed<IViewExecutor>("commonviewex");
        }

        public void Execute(IReportTaskRunContext taskContext)
        {
            var package = taskContext.Packages[Properties.PackageName];

            if (!RunIfVoidPackage && package.DataSets.Count == 0)
                return;

            var packageFileName = (string.IsNullOrEmpty(PackageRename)
                                      ? $@"{Properties.PackageName}"
                                      : taskContext.SetStringParameters(PackageRename))
                                  + (DateInName
                                      ? $" {DateTime.Now:dd.MM.yy}"
                                      : null);

            var uri = new Uri(Host);
            SessionOptions sessionOptions = new SessionOptions
            {
                Protocol = Protocol.Ftp,
                HostName = uri.Host,
                UserName = Login,
                Password = Password
            };

            using (Session session = new Session())
            {
#pragma warning disable 618
                session.DisableVersionCheck = true;
#pragma warning restore 618
                session.Open(sessionOptions);

                if (ClearInterval > 0)
                    CleanupFolder(session);

                if (!string.IsNullOrEmpty(FileName) && !string.IsNullOrEmpty(SourceFileFolder))
                    SaveFileToServer(taskContext, session);
            }

            //winscp do not support stream uploading
            var credentials = new NetworkCredential(Login, Password);

            if (ConvertPackageToJson)
                SaveJsonPackageToServer(package, packageFileName, credentials);

            if (ConvertPackageToXlsx)
                SaveXlsxPackageToServer(package, packageFileName, credentials);

            if (ConvertPackageToCsv)
                SaveCsvPackageToServer(package, packageFileName, credentials);
        }

        private void CleanupFolder(Session session)
        {
            RemoteDirectoryInfo directoryInfo = session.ListDirectory(FolderPath);
            var files = directoryInfo.Files.Where(file => !file.IsDirectory).ToList();

            var cutOff = DateTime.Now.AddDays(-ClearInterval);
            var oldfiles = files.Where(file => file.LastWriteTime < cutOff).ToList();

            foreach (var file in oldfiles)
                session.RemoveFiles(file.FullName);
        }

        private void SaveFileToServer(IReportTaskRunContext taskContext, Session session)
        {
            var fullPath = Path.Combine(
                SourceFileFolder == "Default folder" ? taskContext.DataFolderPath : SourceFileFolder,
                FileName);

            var uri = Path.Combine(FolderPath, FileName);

            TransferOptions transferOptions = new TransferOptions
            {
                TransferMode = TransferMode.Binary
            };

            var transferResult = session.PutFiles(fullPath, uri, false, transferOptions);
            transferResult.Check();
        }

        private void SaveJsonPackageToServer(OperationPackage package, string packageFileName,
            NetworkCredential credentials)
        {
            var parser = autofac.Resolve<IPackageParser>();

            var sets = parser.GetPackageValues(package);

            var dataToSave = UseAllSets
                ? JsonConvert.SerializeObject(sets)
                : JsonConvert.SerializeObject(sets.First());

            var packageNameJson = packageFileName
                                  + ".json";
            var uri = Path.Combine(Host, FolderPath, packageNameJson);

            if (WebRequest.Create(uri) is FtpWebRequest request)
            {
                request.Method = WebRequestMethods.Ftp.UploadFile;
                request.Credentials = credentials;

                var byteds = Encoding.UTF8
                    .GetBytes(dataToSave);

                using Stream requestStream = request.GetRequestStream();

                requestStream.Write(byteds, 0, byteds.Length);
            }
        }

        private void SaveXlsxPackageToServer(OperationPackage package, string packageFileName,
            NetworkCredential credentials)
        {
            var packageNameXlsx = packageFileName
                                  + ".xlsx";

            var uri = Path.Combine(Host, FolderPath, packageNameXlsx);

            if (WebRequest.Create(uri) is FtpWebRequest request)
            {
                request.Method = WebRequestMethods.Ftp.UploadFile;
                request.Credentials = credentials;

                using var excel = viewExecutor.ExecuteXlsx(package, Properties.PackageName, UseAllSets);
                using Stream requestStream = request.GetRequestStream();

                excel.SaveAs(requestStream);
            }
        }

        private void SaveCsvPackageToServer(OperationPackage package, string packageFileName,
            NetworkCredential credentials)
        {
            var packageNameCsv = packageFileName
                                 + ".csv";
            var csvBytes = viewExecutor.ExecuteCsv(package, useAllSets: UseAllSets);

            var uri = Path.Combine(Host, FolderPath, packageNameCsv);

            if (WebRequest.Create(uri) is FtpWebRequest request)
            {
                request.Method = WebRequestMethods.Ftp.UploadFile;
                request.Credentials = credentials;

                using Stream requestStream = request.GetRequestStream();

                requestStream.Write(csvBytes, 0, csvBytes.Length);
            }
        }

        public Task ExecuteAsync(IReportTaskRunContext taskContext)
        {
            Execute(taskContext);
            return Task.CompletedTask;
        }
    }
}