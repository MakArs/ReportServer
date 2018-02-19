using System.Collections.Generic;

namespace ReportService.Interfaces
{
    public class ReportTask
    {
        public int ID { get; set; }
        public string SendAddress { get; set; }
        public int ViewTemplateID { get; set; }
        public int ScheduleID { get; set; }
    }

    public interface IConfig
    {
        int SaveInstance(int taskID, string json, string html);
        void Reload();
        List<ReportTask> GetTasks();
    }
}
