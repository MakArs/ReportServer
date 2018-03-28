using System.Collections.Generic;
using System.Linq;
using Gerakul.FastSql;
using ReportService.Interfaces;

namespace ReportService.Core
{

    public class Repository : IRepository
    {
        private readonly string _connStr;

        public Repository(string connStr)
        {
            _connStr = connStr;
        }

        public List<DTOInstanceCompact> GetCompactInstancesByTaskId(int taskId)
        {
            return SimpleCommand.ExecuteQuery<DTOInstanceCompact>(_connStr,
                    $"select id,taskid,starttime,duration,state,trynumber from Instance where taskid={taskId}")
                .ToList();
        }

        public DTOInstance GetInstanceById(int id)
        {
            return SimpleCommand.ExecuteQuery<DTOInstance>(_connStr,
                $"select * from Instance where id={id}").ToList().First();
        }

        public List<DTOInstanceCompact> GetAllCompactInstances()
        {
            return SimpleCommand.ExecuteQuery<DTOInstanceCompact>(_connStr,
                    "select id,taskid,starttime,duration,state,trynumber from Instance")
                .ToList();
        }

        public List<DTOInstance> GetInstancesByTaskId(int taskId)
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

        public void DeleteInstance(int instanceId)
        {
            SimpleCommand.ExecuteNonQuery(_connStr, $@"delete Instance where id={instanceId}");
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
                State int not null,
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
