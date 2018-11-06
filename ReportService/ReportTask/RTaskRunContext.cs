using System.Collections.Generic;
using ReportService.Interfaces.ReportTask;

namespace ReportService.ReportTask
{
    public class RTaskRunContext : IRTaskRunContext
    {
        public Dictionary<string, OperationPackage> Packages { get; set; } =
            new Dictionary<string, OperationPackage>();

        public int TaskId { get; set; }
        public string TaskName { get; set; }
        public IDefaultTaskExporter exporter { get; set; }
    }
}