using System.Collections.Generic;
using System.Linq;
using Gerakul.FastSql;
using ReportService.Interfaces;

namespace ReportService.Implementations
{
    public class DTOTask
    {
        public int Id { get; set; }
        public string Schedule { get; set; }
        public  string ConnectionString { get; set; }
        public string ViewTemplate { get; set; }
        public string Query { get; set; }
        public string SendAddress { get; set; }
        public int TryCount { get; set; }
        public int QueryTimeOut { get; set; } //seconds
        public int TaskType { get; set; }
    }

    public class Repository : IRepository
    {
        private string connStr = @"Data Source=WS-00005; Initial Catalog=ReportBase; Integrated Security=True";

        public Repository()
        {
        }

        public int CreateInstance(int taskId, string json, string html, double duration, string state, int tryNumber)
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
                    values ('{json.Replace("'", "''")}',
                    '{html.Replace("'", "''")}',
                    {taskId},
                    getdate(),
                    {duration},
                    '{state}',
                    {tryNumber}); select cast(scope_identity() as int)")
                    .First();
        }

        public void UpdateInstance(int instanceId, string json, string html, double duration, string state, int tryNumber)
        {
            // TODO:DTO_Instance xx = ....;catch error with updating db(formatting...)
            SimpleCommand.ExecuteNonQuery(connStr,
                 $@"Update Instance
                    set  Data='{json.Replace("'", "''")}',
                    ViewData='{html.Replace("'", "''")}',
                    Duration={duration},
                    State='{state}',
                    TryNumber={tryNumber}
                    where id={instanceId}");
        }

        public List<DTOTask> GetTasks()
        {
            return SimpleCommand.ExecuteQuery<DTOTask>(connStr, @"select * from task").ToList(); ;
        }

        public void UpdateTask(int taskId, DTOTask task)
        {
            MappedCommand.Update(connStr, "Task", task, "Id");
        }

        public void DeleteTask(int taskId)
        {
            SimpleCommand.ExecuteNonQuery(connStr, $@"delete Task where id={taskId}");
        }

        public int CreateTask(DTOTask task)
        {
            var id = MappedCommand.InsertAndGetId(connStr, "Task", task, "Id");
            return (int)id;
        }

        public void CreateBase(string baseConnStr)
        {
                SimpleCommand.ExecuteNonQuery(baseConnStr, $@"create table Instance
                (
                Id int primary key Identity,
                Data nvarchar(MAX) not null,
                ViewData nvarchar(MAX) not null,
                TaskID int not null,
                StartTime datetime not null,
                Duration int not null,
                State nvarchar(255) not null,
                TryNumber int not null
                )");

                SimpleCommand.ExecuteNonQuery(baseConnStr, $@"create table Task
                (Id int primary key Identity,
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
    }//class
}
