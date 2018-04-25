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
        } //ctor

        public List<DtoRecepientGroup> GetAllRecepientGroups()
        {
            return SimpleCommand.ExecuteQuery<DtoRecepientGroup>(_connStr,
                    "select * from RecepientGroup")
                .ToList();
        }

        public List<DtoSchedule> GetAllSchedules()
        {
            return SimpleCommand.ExecuteQuery<DtoSchedule>(_connStr,
                    "select * from Schedule")
                .ToList();
        }

        public List<DtoReport> GetAllReports()
        {
            return SimpleCommand.ExecuteQuery<DtoReport>(_connStr,
                    "select * from Report")
                .ToList();
        }

        public List<DtoTask> GetAllTasks()
        {
            return SimpleCommand.ExecuteQuery<DtoTask>(_connStr, "select * from task").ToList();
        }

        public List<DtoInstance> GetAllInstances()
        {
            return SimpleCommand.ExecuteQuery<DtoInstance>(_connStr,
                    "select * from Instance")
                .ToList();
        }

        public List<DtoInstance> GetInstancesByTaskId(int taskId)
        {
            return SimpleCommand.ExecuteQuery<DtoInstance>(_connStr,
                    $"select * from Instance where taskid={taskId}")
                .ToList();
        }

        public List<DtoFullInstance> GetFullInstancesByTaskId(int taskId)
        {
            return SimpleCommand.ExecuteQuery<DtoFullInstance>(_connStr,
                    $@"select id,taskid,starttime,duration,state,trynumber,data,viewdata
                from Instance i join instancedata idat on id=instanceid where taskid={taskId} order by id")
                .ToList();
        }

        public DtoFullInstance GetFullInstanceById(int id)
        {
            return SimpleCommand.ExecuteQuery<DtoFullInstance>(_connStr,
                    $@"select id,taskid,starttime,duration,state,trynumber,data,viewdata
                from Instance i join instancedata idat on id=instanceid where id={id}")
                .ToList().First();
        }

        public int CreateEntity<T>(T entity)
        {
            switch (entity)
            {
                case DtoRecepientGroup recepgroup:
                    return (int) MappedCommand.InsertAndGetId(_connStr, "RecepientGroup", recepgroup, "Id");

                case DtoSchedule sched:
                    return (int) MappedCommand.InsertAndGetId(_connStr, "Schedule", sched, "Id");

                case DtoReport rep:
                    return (int) MappedCommand.InsertAndGetId(_connStr, "Report", rep, "Id");

                case DtoTask task:
                    return (int) MappedCommand.InsertAndGetId(_connStr, "Task", task, "Id");

                case DtoInstance instance:
                    return (int) MappedCommand.InsertAndGetId(_connStr, "Instance", instance, "Id");

                case DtoInstanceData instanceData:
                {
                    MappedCommand.Insert(_connStr, "InstanceData", instanceData);
                    return 0;
                }
                default:
                    return 0;
            }
        }

        public void UpdateEntity<T>(T entity)
        {
            switch (entity)
            {
                case DtoRecepientGroup recepgroup:
                    MappedCommand.Update(_connStr, "RecepientGroup", recepgroup, "Id");
                    break;

                case DtoSchedule sched:
                    MappedCommand.Update(_connStr, "RecepientGroup", sched, "Id");
                    break;

                case DtoReport rep:
                    MappedCommand.Update(_connStr, "Report", rep, "Id");
                    break;

                case DtoTask task:
                    MappedCommand.Update(_connStr, "Task", task, "Id");
                    break;

                case DtoInstance instance:
                    MappedCommand.Update(_connStr, "Instance", instance, "Id");
                    break;

                case DtoInstanceData instanceData:
                    MappedCommand.Update(_connStr, "InstanceData", instanceData, "InstanceId");
                    break;
            }
        }
        
        public void DeleteRecepientGroup(int groupId)
        {
            throw new NotImplementedException();
        }

        public void DeleteSchedule(int scheduleId)
        {
            throw new NotImplementedException();
        }


        public void DeleteInstance(int instanceId)
        {
            SimpleCommand.ExecuteNonQuery(_connStr, $@"delete InstanceData where instanceid={instanceId}");
            SimpleCommand.ExecuteNonQuery(_connStr, $@"delete Instance where id={instanceId}");
        }


        public void DeleteTask(int taskId)
        {
            SimpleCommand.ExecuteNonQuery(_connStr, $@"delete InstanceData where instanceid in (select id from instance where TaskID={taskId})");
            SimpleCommand.ExecuteNonQuery(_connStr, $@"delete Instance where TaskID={taskId}");
            SimpleCommand.ExecuteNonQuery(_connStr, $@"delete Task where id={taskId}");
        }

        public void CreateBase(string baseConnStr)
        {
            // TODO: check db exists ~find way to cut redundant code 
            // TODO: refactoring
            // Task table refac (new base-creating uses)
            // 4. ConnStr => DataSource table
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
                IF OBJECT_ID('Report') IS NULL
                CREATE TABLE Report
                (Id INT PRIMARY KEY IDENTITY,
                Name NVARCHAR(127) NULL,
                ConnectionString NVARCHAR(255) NULL,
                ViewTemplate NVARCHAR(MAX) NOT NULL,
                Query NVARCHAR(MAX) NOT NULL,
                ReportType TINYINT NOT NULL,
                QueryTimeOut SMALLINT NOT NULL
                ); ");

            SimpleCommand.ExecuteNonQuery(baseConnStr, $@"
                IF OBJECT_ID('Task') IS NULL
                CREATE TABLE Task
                (Id INT PRIMARY KEY IDENTITY,
                ReportId INT NOT NULL,
                ScheduleId INT NULL,
                RecepientGroupId INT NULL,
                TryCount TINYINT NOT NULL,
                CONSTRAINT FK_Task_RecepientGroup FOREIGN KEY(RecepientGroupId) 
                REFERENCES RecepientGroup(Id),
                CONSTRAINT FK_Task_Schedule FOREIGN KEY(ScheduleId) 
                REFERENCES Schedule(Id),
                CONSTRAINT FK_Task_Report FOREIGN KEY(ReportId) 
                REFERENCES Report(Id)
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
	            Data NVARCHAR(MAX) NULL,
	            ViewData NVARCHAR(MAX) NULL,
                CONSTRAINT FK_InstanceData_Data FOREIGN KEY(InstanceId)
                REFERENCES Instance(Id)
                )");
        }
    } //class
}
