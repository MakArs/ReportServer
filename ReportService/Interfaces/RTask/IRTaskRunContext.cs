using System.Collections.Generic;

namespace ReportService.Interfaces.RTask
{
    public interface IRTaskRunContext
    {
        Dictionary<string, string> DataSets { get; set; }
        int TaskId { get; set; }
        string TaskName { get; set; }
        IDefaultTaskExporter exporter { get; set; }
    }
}
