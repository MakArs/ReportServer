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

        private readonly IArchiver archiver;

        public RTaskRunContext(IArchiver archiver)
        {
            this.archiver = archiver;
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