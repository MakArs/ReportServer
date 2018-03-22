using System;
using System.Collections.Generic;
using System.Linq;
using Gerakul.FastSql;
using ReportService.Interfaces;

namespace ReportService.Implementations
{
    public class DTO_Task
    {
        public int ID { get; set; }
        public string Schedule { get; set; }
        public  string ConnectionString { get; set; }
        public string ViewTemplate { get; set; }
        public string Query { get; set; }
        public string SendAddress { get; set; }
        public int TryCount { get; set; }
        public int QueryTimeOut { get; set; } //seconds
        public int TaskType { get; set; }
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

        public int CreateInstance(int ataskID, string ajson, string ahtml, double aduration, string astate, int atryNumber)
        {
            return SimpleCommand.ExecuteQueryFirstColumn<int>(connStr,
                 $@"INSERT INTO Instance
                  (
                    Data,
                    ViewData,
                    TaskID,
                    StartTime,
                    Duration,
                    State,
                    TryNumber
                    )  
                    values ('{ajson.Replace("'", "''")}',
                    '{ahtml.Replace("'", "''")}',
                    {ataskID},
                    getdate(),
                    {aduration},
                    '{astate}',
                    {atryNumber}); select cast(scope_identity() as int)")
                    .First();
        }

        public void UpdateInstance(int ainstanceID, string ajson, string ahtml, double aduration, string astate, int atryNumber)
        {
            SimpleCommand.ExecuteNonQuery(connStr,
                 $@"Update Instance
                    set  Data='{ajson.Replace("'", "''")}',
                    ViewData='{ahtml.Replace("'", "''")}',
                    Duration={aduration},
                    State='{astate}',
                    TryNumber={atryNumber}
                    where id={ainstanceID}");
        }

        public List<DTO_Task> GetTasks()
        {
            return Tasks;
        }

        public void UpdateTask(int ataskID, DTO_Task atask)
        {
                SimpleCommand.ExecuteNonQuery(connStr, $@"update Task
                set Schedule='{atask.Schedule}',
                ConnectionString ='{atask.ConnectionString}',
                ViewTemplate='{atask.ViewTemplate}',
                Query='{atask.Query}',
                SendAddress='{atask.SendAddress}',
                TryCount={atask.TryCount},
                QueryTimeOut={atask.QueryTimeOut},
                TaskType={atask.TaskType}
                where id={ataskID}");
        }

        public void DeleteTask(int ataskID)
        {
            SimpleCommand.ExecuteNonQuery(connStr, $@"delete Task where id={ataskID}");
        }

        public int CreateTask(DTO_Task atask)
        {
           return SimpleCommand.ExecuteQueryFirstColumn<int>(connStr, $@"INSERT INTO Task
                (Schedule,
                ConnectionString,
                ViewTemplate,
                Query,
                SendAddress,
                TryCount,
                QueryTimeOut,
                TaskType
                )
                values
                ('{atask.Schedule}',
                '{atask.ConnectionString}',
                '{atask.ViewTemplate}',
                '{atask.Query}',
                '{atask.SendAddress}',
                {atask.TryCount},
                {atask.QueryTimeOut},
                {atask.TaskType}
                );
                select cast(scope_identity() as int)").First();
        }

        public void CreateBase(string abaseConnStr)
        {
                SimpleCommand.ExecuteNonQuery(abaseConnStr, $@"create table Instance
                (
                ID int primary key Identity,
                Data nvarchar(MAX) not null,
                ViewData nvarchar(MAX) not null,
                TaskID int not null,
                StartTime datetime not null,
                Duration int not null,
                State nvarchar(255) not null,
                TryNumber int not null
                )");

                SimpleCommand.ExecuteNonQuery(abaseConnStr, $@"create table Task
                (ID int primary key Identity,
                Schedule nvarchar(255) not null,
                ConnectionString nvarchar(255) null,
                ViewTemplate nvarchar(MAX) not null,
                Query nvarchar(MAX) not null,
                SendAddress varchar(4000) not null,
                TryCount TINYINT not null,
                QueryTimeOut TINYINT not null,
                TaskType TINYINT not null 
                )");
        }
    }
}
