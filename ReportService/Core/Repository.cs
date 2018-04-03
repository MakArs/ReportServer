using System;
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

        public List<DtoInstanceCompact> GetCompactInstancesByTaskId(int taskId)
        {
            return SimpleCommand.ExecuteQuery<DtoInstanceCompact>(_connStr,
                    $"select * from Instance where taskid={taskId}")
                .ToList();
        }

        public DtoInstance GetInstanceById(int id)
        {
            return SimpleCommand.ExecuteQuery<DtoInstance>(_connStr,
                    $@"select id,taskid,starttime,duration,state,trynumber,data,viewdata
                from Instance i join instancedata idat on id=instanceid where id={id}")
                .ToList().First();
        }

        public List<DtoInstanceCompact> GetAllCompactInstances()
        {
            return SimpleCommand.ExecuteQuery<DtoInstanceCompact>(_connStr,
                    "select * from Instance")
                .ToList();
        }

        public List<DtoSchedule> GetAllSchedules()
        {
                return SimpleCommand.ExecuteQuery<DtoSchedule>(_connStr,
                        "select * from Schedule")
                    .ToList();
        }

        public List<DtoInstance> GetInstancesByTaskId(int taskId)
        {
            return SimpleCommand.ExecuteQuery<DtoInstance>(_connStr,
                    $@"select id,taskid,starttime,duration,state,trynumber,data,viewdata
                from Instance i join instancedata idat on id=instanceid where taskid={taskId}")
                .ToList();
        }

        public void UpdateInstance(DtoInstanceCompact instance, DtoInstanceData data)
        {
            MappedCommand.Update(_connStr, "Instance", instance, "Id");
            MappedCommand.Update(_connStr, "InstanceData", data, "InstanceId");
        }

        public int CreateInstance(DtoInstanceCompact instance, DtoInstanceData data)
        {
            var id = MappedCommand.InsertAndGetId(_connStr, "Instance", instance, "Id");
            data.InstanceId = (int) id;
                MappedCommand.Insert(_connStr, "InstanceData", data);
            return (int) id;
        }

        public void DeleteInstance(int instanceId)
        {
            SimpleCommand.ExecuteNonQuery(_connStr, $@"delete InstanceData where instanceid={instanceId}");
            SimpleCommand.ExecuteNonQuery(_connStr, $@"delete Instance where id={instanceId}");
        }

        public List<DtoTask> GetTasks()
        {
            return SimpleCommand.ExecuteQuery<DtoTask>(_connStr, "select * from task").ToList();
        }

        public void UpdateTask(DtoTask task)
        {
            MappedCommand.Update(_connStr, "Task", task, "Id");
        }

        public void DeleteTask(int taskId)
        {
            SimpleCommand.ExecuteNonQuery(_connStr, $@"delete Task where id={taskId}");
        }

        public int CreateTask(DtoTask task)
        {
            var id = MappedCommand.InsertAndGetId(_connStr, "Task", task, "Id");
            return (int) id;
        }

        public void CreateBase(string baseConnStr)
        {
            // TODO: check db exists

            SimpleCommand.ExecuteNonQuery(baseConnStr, $@"create table Schedule
                (Id int primary key Identity,
                Name nvarchar(127) not null,
                Schedule nvarchar(255) not null
                )");

            // TODO: insert default schedules (english)

            SimpleCommand.ExecuteNonQuery(baseConnStr, $@"CREATE TABLE Task
                (Id int primary key Identity,
                ScheduleId int null,
                ConnectionString nvarchar(255) null,
                ViewTemplate nvarchar(MAX) not null,
                Query nvarchar(MAX) not null,
                SendAddresses varchar(4000) not null,
                TryCount TINYINT not null,
                QueryTimeOut TINYINT not null,
                TaskType TINYINT not null

                CONSTRAINT FK_Task_Schedule FOREIGN KEY(ScheduleId) REFERENCES Schedule(Id)
                )");
            
            SimpleCommand.ExecuteNonQuery(baseConnStr, $@"create table Instance
                (
                Id int primary key Identity,
                TaskID int not null,
                StartTime datetime not null,
                Duration int not null,
                State int not null,
                TryNumber int not null,
                constraint FK_Instance_Task FOREIGN KEY(TaskID)
                REFERENCES Task(Id)
                )");

            SimpleCommand.ExecuteNonQuery(baseConnStr, $@"CREATE TABLE [dbo].[InstanceData](
	            InstanceId int NOT NULL,
	            Data nvarchar(MAX) not null,
	            ViewData nvarchar(MAX) not null,
                constraint FK_InstanceData_Data FOREIGN KEY(InstanceId)
                REFERENCES Instance(Id)
                )");

            // TODO: refactoring
            // Task table refac
            // 1. Schedule to external table: Id, Name, Value
            // 2. SendAddresses to ex table RecepientGroup: Id, Name, Emails
            // 3. ConnStr, Templ, Query, TaskType => Report table
            // 4. ConnStr => DataSource table
        }
    } //class
}
