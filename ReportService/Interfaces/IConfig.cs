using System.Collections.Generic;

namespace ReportService.Interfaces
{
    public class ReportTask
    {
        public int TaskID { get; set; }
        public string Query { get; set; }
        public int ViewID { get; set; }
        public int ScheduleID { get; set; }
    }

    public interface IConfig
    {
        int SaveInstance(int taskID, string json, string html);
        void Reload();
        List<ReportTask> GetTasks();
    }
}
