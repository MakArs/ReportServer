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

        public List<DtoRecepientGroup> GetAllRecepientGroups()
        {
            return SimpleCommand.ExecuteQuery<DtoRecepientGroup>(_connStr,
                    "select * from RecepientGroup")
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
            // TODO: check db exists ~find way to cut redundant code 

            SimpleCommand.ExecuteNonQuery(baseConnStr, $@"
                IF OBJECT_ID('RecepientGroup') IS NULL
                CREATE TABLE RecepientGroup
                (Id INT PRIMARY KEY IDENTITY,
                Name NVARCHAR(127) NOT NULL,
                Addresses NVARCHAR(4000) NOT NULL
                ); ");

            var existScheduleTable = Convert.ToInt64(SimpleCommand
                .ExecuteQueryFirstColumn<object>(baseConnStr, $@"
               SELECT ISNULL(OBJECT_ID('Schedule'),0)")
                .First());
            if (existScheduleTable == 0)
            {
                var schedules = new[]
                {
                    new DtoSchedule() {Name = "workDaysEvening", Schedule = "motuwethfr2230"},
                    new DtoSchedule() {Name = "sundayEvening", Schedule = "su2230"}
                };
                SimpleCommand.ExecuteNonQuery(baseConnStr, $@"
                CREATE TABLE Schedule
                (Id INT PRIMARY KEY IDENTITY,
                Name NVARCHAR(127) NOT NULL,
                Schedule NVARCHAR(255) NOT NULL
                )");
                schedules.WriteToServer(baseConnStr, "Schedule");
            }

            SimpleCommand.ExecuteNonQuery(baseConnStr, $@"
                IF OBJECT_ID('Task') IS NULL
                CREATE TABLE Task
                (Id INT PRIMARY KEY IDENTITY,
                ScheduleId INT NULL,
                ConnectionString NVARCHAR(255) NULL,
                ViewTemplate NVARCHAR(MAX) NOT NULL,
                Query NVARCHAR(MAX) NOT NULL,
                RecepientGroupId INT NULL,
                TryCount TINYINT NOT NULL,
                QueryTimeOut TINYINT NOT NULL,
                TaskType TINYINT NOT NULL
                CONSTRAINT FK_Task_RecepientGroup FOREIGN KEY(RecepientGroupId) REFERENCES RecepientGroup(Id),
                CONSTRAINT FK_Task_Schedule FOREIGN KEY(ScheduleId) REFERENCES Schedule(Id)
                )");

            SimpleCommand.ExecuteNonQuery(baseConnStr, $@"
                IF OBJECT_ID('Instance') IS NULL
                CREATE TABLE Instance
                (
                Id INT PRIMARY KEY IDENTITY,
                TaskID INT NOT NULL,
                StartTime DATETIME NOT NULL,
                Duration INT NOT NULL,
                State INT NOT NULL,
                TryNumber INT NOT NULL,
                CONSTRAINT FK_Instance_Task FOREIGN KEY(TaskID)
                REFERENCES Task(Id)
                )");

            SimpleCommand.ExecuteNonQuery(baseConnStr, $@"
                IF object_id('InstanceData') IS NULL
                CREATE TABLE InstanceData(
	            InstanceId INT NOT NULL,
	            Data NVARCHAR(MAX) NOT NULL,
	            ViewData NVARCHAR(MAX) NOT NULL,
                CONSTRAINT FK_InstanceData_Data FOREIGN KEY(InstanceId)
                REFERENCES Instance(Id)
                )");

            // TODO: refactoring
            // Task table refac
            // 3. ConnStr, Templ, Query, TaskType => Report table
            // 4. ConnStr => DataSource table
        }
    } //class
}
