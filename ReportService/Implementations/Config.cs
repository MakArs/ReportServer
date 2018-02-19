using System.Collections.Generic;
using System.Linq;
using Gerakul.FastSql;
using ReportService.Interfaces;

namespace ReportService.Models
{
    public class Config : IConfig
    {
        public List<ReportTask> Tasks { get; private set; }
        private string connStr = @""; //TODO: add connstr

        public Config()
        {
            Tasks = SimpleCommand.ExecuteQuery<ReportTask>(connStr, @"select * from tasks").ToList();//TODO: test on database
        }

        public void Reload()
        {
            Tasks = null;
            Tasks = SimpleCommand.ExecuteQuery<ReportTask>(connStr, @"select * from tasks").ToList();
        }

        public int SaveInstance(int taskID, string json, string html)
        {
            var t = SimpleCommand.ExecuteQueryFirstColumn(connStr,
                 $@"INSERT INTO Instance
                  (TaskId,
                    Data,
                    View,
                    Date
                    )  
                    values ({taskID},
                    '{json}',
                    '{html}',
                    getdate());")
                    ;
            return SimpleCommand.ExecuteQuery<int>(connStr, @"select max(id) from instance").ToArray().First();//TODO: test on database
        }

        public List<ReportTask> GetTasks()
        {
            return Tasks;
        }
    }
}
