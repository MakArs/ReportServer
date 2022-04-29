using ReportService.Entities;

namespace ReportService.Api.Models
{
    public class RunTaskParameters
    {
        public long TaskId { get; set; }
        public TaskParameter[] Parameters { get; set; }
    }
}
