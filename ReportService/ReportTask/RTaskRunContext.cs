using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using Google.Protobuf;
using ReportService.Interfaces.Core;
using ReportService.Interfaces.ReportTask;

namespace ReportService.ReportTask
{
    public class RTaskRunContext : IRTaskRunContext
    {
        public Dictionary<string, OperationPackage> Packages { get; set; } =
            new Dictionary<string, OperationPackage>();

        public List<string> PackageStates { get; set; }

        public List<IOperation> OpersToExecute { get; set; }

        public int TaskId { get; set; }
        public DtoTaskInstance TaskInstance { get; set; }
        public CancellationTokenSource CancelSource { get; set; }
        public string TaskName { get; set; }
        public IDefaultTaskExporter Exporter { get; set; }
        public Dictionary<string, object> Parameters { get; set; }

        public string DataFolderPath => Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) +
                                        $@"\ReportServer\{TaskInstance.Id}";

        private readonly IArchiver archiver;

        public RTaskRunContext(IArchiver archiver)
        {
            this.archiver = archiver;
        }

        public void CreateDataFolder()
        {
            Directory.CreateDirectory(DataFolderPath);
        }

        public void RemoveDataFolder()
        {
            DirectoryInfo di = new DirectoryInfo(DataFolderPath);

            foreach (FileInfo file in di.GetFiles())
            {
                file.Delete();
            }

            foreach (DirectoryInfo dir in di.GetDirectories())
            {
                dir.Delete(true);
            }

            di.Delete();
        }

        public byte[] GetCompressedPackage(string packageName)
        {
            using (var stream = new MemoryStream())
            {
                Packages[packageName].WriteTo(stream);
                return archiver.CompressStream(stream);
            }
        }
    }
}