using System.Collections.Generic;
using System.Threading;
using ReportService.Interfaces.Core;
using ReportService.Interfaces.ReportTask;

namespace ReportService.ReportTask
{
    public class RTaskRunContext : IRTaskRunContext
    {
        public Dictionary<string, OperationPackage> Packages { get; set; } =
            new Dictionary<string, OperationPackage>();

        public List<IOperation> OpersToExecute { get; set; }

        public int TaskId { get; set; }
        public DtoTaskInstance TaskInstance { get; set; }
        public CancellationTokenSource CancelSource { get; set; }
        public string TaskName { get; set; }
        public IDefaultTaskExporter Exporter { get; set; }
        public Dictionary<string, object> Parameters { get; set; }
    }
}