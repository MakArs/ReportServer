using System.Collections.Generic;
using System.Linq;
using Gerakul.FastSql;
using ReportService.Interfaces;

namespace ReportService.Models
{
    public class Config : IConfig
    {
        public List<ReportTask> Tasks { get; private set; }
        private string connStr =@""; //+connstr
        
        public Config()
        {
            Tasks = SimpleCommand.ExecuteQuery<ReportTask>(connStr, @"select * from tasks").ToList();//+test
        }

        public void Reload()
        {
            Tasks = null;
            Tasks = SimpleCommand.ExecuteQuery<ReportTask>(connStr, @"select * from tasks").ToList();
        }

        public int CreateInstance(int taskID, string json, string html)
        {
            string t = SimpleCommand.ExecuteQueryFirstColumn<string>(connStr,
                 $@"INSERT INTO Instance
                  (TaskId,
                    Data,
                    View,
                    Date
                    )  
                    values ({taskID},
                    '{json}',
                    '{html}',
                    getdate());  select scope_identity() ")
                    .First();
            return SimpleCommand.ExecuteQuery<int>(connStr, @"select max(id) from instance").ToArray().First();//+test
        }

        public List<ReportTask> GetTasks()
        {
            return Tasks;
        }
    }
}
