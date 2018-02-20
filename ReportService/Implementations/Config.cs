using System.Collections.Generic;
using System.Linq;
using Gerakul.FastSql;
using ReportService.Interfaces;

namespace ReportService.Implementations
{
    public class ConfigTest : IConfig
    {
        public List<ReportTask> Tasks { get; private set; }
        private string connStr = @"Data Source=WS-00005; Initial Catalog=ReportBase; Integrated Security=True";

        public ConfigTest()
        {
            Tasks = SimpleCommand.ExecuteQuery<ReportTask>(connStr, @"select * from task_").ToList();
        }

        public void Reload()
        {
            Tasks = null;
            Tasks = SimpleCommand.ExecuteQuery<ReportTask>(connStr, @"select * from task_").ToList();
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
                    .First();//TODO: test on database with json+html formats; 
        }

        public List<ReportTask> GetTasks()
        {
            return Tasks;
        }
    }
}
