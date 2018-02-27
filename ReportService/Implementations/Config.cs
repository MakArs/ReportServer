using System.Collections.Generic;
using System.Linq;
using Gerakul.FastSql;
using ReportService.Interfaces;

namespace ReportService.Implementations
{
    public class DTO_Task
    {
        public int ID { get; set; }
        public string SendAddress { get; set; }
        public int ViewTemplateID { get; set; }
        public int ScheduleID { get; set; }
        public string Query { get; set; }
    }

    public class ConfigTest : IConfig
    {
        public List<DTO_Task> Tasks { get; private set; }
        private string connStr = @"Data Source=WS-00005; Initial Catalog=ReportBase; Integrated Security=True";

        public ConfigTest()
        {
            Tasks = SimpleCommand.ExecuteQuery<DTO_Task>(connStr, @"select * from task where id=3").ToList();
        }

        public void Reload()
        {
            Tasks = null;
            Tasks = SimpleCommand.ExecuteQuery<DTO_Task>(connStr, @"select * from task where id=2").ToList();
        }

        public int SaveInstance(int taskID, string json, string html)
        {
            return SimpleCommand.ExecuteQueryFirstColumn<int>(connStr,
                 $@"INSERT INTO Instance
                  (
                    Data,
                    ViewData,
                    TaskID,
                    SaveTime
                    )  
                    values ('{json}',
                    '{html}',
                    {taskID},
                    getdate()); select cast(scope_identity() as int)")
                    .First();
        }

        public List<DTO_Task> GetTasks()
        {
            return Tasks;
        }
    }
}
