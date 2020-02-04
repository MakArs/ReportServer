using System;

namespace ReportService.Entities
{
    public class DependencyState
    {
        public DateTime LastSuccessfulFinish { get; set; }
        public int InProcessCount { get; set; } //todo:check if possible make bool
    }
}