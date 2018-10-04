using System.Collections.Generic;

namespace ReportService.Interfaces
{
    public interface IRTaskRunContext
    {
        Dictionary<string, string> DataSets { get; set; }
    }
}
