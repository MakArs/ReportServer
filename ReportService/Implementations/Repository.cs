using System;
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
        public string ConnectionString { get; set; }
        public string ViewTemplate { get; set; }
        public string Query { get; set; }
        public string SendAddresses { get; set; }
        public int TryCount { get; set; }
        public int QueryTimeOut { get; set; } //seconds
        public int TaskType { get; set; }
    }

    public class DTOInstance
    {
        public int Id { get; set; } = 0;
        public string Data { get; set; } = "";
        public string ViewData { get; set; } = "";
        public int TaskId { get; set; }
        public DateTime StartTime { get; set; }
        public int Duration { get; set; } = 0;
        public string State { get; set; } = "InProcess";
        public int TryNumber { get; set; } = 0;
    }

    public class Repository : IRepository
    {
        private readonly string _connStr;

        public Repository(string connStr)
        {
            _connStr = connStr;
        }

        public List<DTOInstance> GetInstances(int taskId)
        {
            return SimpleCommand.ExecuteQuery<DTOInstance>(_connStr, $"select * from Instance where taskid={taskId}")
                .ToList();
        }

        public void UpdateInstance(DTOInstance instance)
        {
            MappedCommand.Update(_connStr, "Instance", instance, "Id");
        }

        public int CreateInstance(DTOInstance instance)
        {
            var id = MappedCommand.InsertAndGetId(_connStr, "Instance", instance, "Id");
            return (int) id;
        }

        public List<DTOTask> GetTasks()
        {
            return SimpleCommand.ExecuteQuery<DTOTask>(_connStr, "select * from task").ToList();
        }

        public void UpdateTask(DTOTask task)
        {
            MappedCommand.Update(_connStr, "Task", task, "Id");
        }

        public void DeleteTask(int taskId)
        {
            SimpleCommand.ExecuteNonQuery(_connStr, $@"delete Task where id={taskId}");
        }

        public int CreateTask(DTOTask task)
        {
            var id = MappedCommand.InsertAndGetId(_connStr, "Task", task, "Id");
            return (int) id;
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
                SendAddresses varchar(4000) not null,
                TryCount TINYINT not null,
                QueryTimeOut TINYINT not null,
                TaskType TINYINT not null 
                )");
        }
    } //class
}
