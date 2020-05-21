using System;

namespace ReportService.Entities
{
    public class TaskState
    {
        public DateTime LastSuccessfulFinish { get; set; }
        public DateTime LastStart { get; set; }
        public int InProcessCount { get; set; } //todo:check if possible make bool
    }
}