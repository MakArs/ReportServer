using System.Collections.Generic;

namespace ReportService.Api.Models
{
    public class Errors
    {
        public Dictionary<string, string[]> ErrorsInfo { get; set; }
    }
}