using System.Collections.Generic;

namespace ReportService.Interfaces.ReportTask
{
    public interface IRTaskRunContext
    {
        Dictionary<string, OperationPackage> Packages { get; set; }
        int TaskId { get; set; }
        string TaskName { get; set; }
        IDefaultTaskExporter exporter { get; set; }
    }
}
