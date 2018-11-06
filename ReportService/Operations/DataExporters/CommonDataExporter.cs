using ReportService.Interfaces.Core;
using ReportService.Interfaces.ReportTask;

namespace ReportService.Operations.DataExporters
{
    public class CommonDataExporter : IDataExporter
    {
        public int Id { get; set; }
        public bool IsDefault { get; set; }
        public int Number { get; set; }
        public string Name { get; set; }
        public string PackageName { get; set; }
        public bool RunIfVoidPackage { get; set; }

        public virtual void Send(IRTaskRunContext taskContext)
        {
        }

        public virtual void Cleanup(ICleanupSettings cleanUpSettings)
        {
        }
    }
}