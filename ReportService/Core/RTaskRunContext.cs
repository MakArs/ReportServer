using System.Collections.Generic;
using ReportService.Interfaces;

namespace ReportService.Core
{
    public class RTaskRunContext : IRTaskRunContext
    {
        public Dictionary<string, string> DataSets { get; set; } = new Dictionary<string, string>();
    }
}