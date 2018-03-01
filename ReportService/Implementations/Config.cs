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
        public string ViewTemplate { get; set; }
        public string Schedule { get; set; }
        public string Query { get; set; }
        public int TryCount { get; set; }
        public int QueryTimeOut { get; set; }
    }

    public class ConfigTest : IConfig
    {
        public List<DTO_Task> Tasks { get; private set; }
        private string connStr = @"Data Source=WS-00005; Initial Catalog=ReportBase; Integrated Security=True";

        public ConfigTest()
        {
            Tasks = SimpleCommand.ExecuteQuery<DTO_Task>(connStr, @"select * from task").ToList();
        }

        public void Reload()
        {
            Tasks = null;
            Tasks = SimpleCommand.ExecuteQuery<DTO_Task>(connStr, @"select * from task").ToList();
        }

        public int CreateInstance(int ataskID, string ajson, string ahtml, double aduration, int asuccess, int atryNumber)
        {
            return SimpleCommand.ExecuteQueryFirstColumn<int>(connStr,
                 $@"INSERT INTO Instance_
                  (
                    Data,
                    ViewData,
                    TaskID,
                    StartTime,
                    Duration,
                    Success,
                    TryNumber
                    )  
                    values ('{ajson.Replace("'","''")}',
                    '{ahtml.Replace("'","''")}',
                    {ataskID},
                    getdate(),
                    {aduration},
                    {asuccess},
                    {atryNumber}); select cast(scope_identity() as int)")
                    .First();
        }

        public List<DTO_Task> GetTasks()
        {
            return Tasks;
        }
    }
}
