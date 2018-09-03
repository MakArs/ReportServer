using System;
using System.Collections.Generic;
using System.Linq;
using Gerakul.FastSql;
using ReportService.Interfaces;

namespace ReportService.Core
{
    public class Repository : IRepository
    {
        private readonly string connStr;

        public Repository(string connStr)
        {
            this.connStr = connStr;
        } //ctor

        public List<T> GetListEntitiesByDtoType<T>() where T : new()
        {
            var tableName = typeof(T).Name.Remove(0, 3);
            try
            {
                return SimpleCommand.ExecuteQuery<T>(connStr, $"select * from {tableName}")
                    .ToList();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return null;
            }
        }
        
        public List<DtoInstance> GetInstancesByTaskId(int taskId)
        {
            return SimpleCommand.ExecuteQuery<DtoInstance>(connStr,
                    $"select * from Instance where taskid={taskId}")
                .ToList();
        }

        public List<DtoFullInstance> GetFullInstancesByTaskId(int taskId)
        {
            return SimpleCommand.ExecuteQuery<DtoFullInstance>(connStr,
                    $@"select id,taskid,starttime,duration,state,trynumber,data,viewdata
                from Instance i join instancedata idat on id=instanceid where taskid={taskId} order by id")
                .ToList();
        }

        public DtoFullInstance GetFullInstanceById(int id)
        {
            return SimpleCommand.ExecuteQuery<DtoFullInstance>(connStr,
                    $@"select id,taskid,starttime,duration,state,trynumber,data,viewdata
                from Instance i join instancedata idat on id=instanceid where id={id}")
                .ToList().First();
        }
        
        public int CreateEntity<T>(T entity)
        {
            if (entity is DtoInstanceData instanceData)
            {
                MappedCommand.Insert(connStr, "InstanceData", instanceData);
                       return 0;
            }

            var tableName = typeof(T).Name.Remove(0, 3);

            try
            {
            return (int)MappedCommand.InsertAndGetId(connStr, $"{tableName}", entity, "Id");
            }

            catch (Exception e)
            {
                Console.WriteLine(e);
                return 0;
            }
        }

        public void UpdateEntity<T>(T entity)
        {
            if (entity is DtoInstanceData instanceData)
            {
                MappedCommand.Update(connStr, "InstanceData", instanceData, "InstanceId");
            }

            var tableName = typeof(T).Name.Remove(0, 3);

            try
            {
                MappedCommand.Update(connStr, $"{tableName}", entity, "Id");
            }

            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        public void DeleteEntity<T>(int id)
        {
            var type = typeof(T);
            switch (true)
            {
                case bool _ when type == typeof(DtoExporterConfig): //todo:method
                    break;

                case bool _ when type == typeof(DtoSchedule): //todo:method
                    break;

                case bool _ when type == typeof(DtoExporterToTaskBinder): //todo:method
                    break;

                case bool _ when type == typeof(DtoTelegramChannel): //todo:method
                    break;

                case bool _ when type == typeof(DtoRecepientGroup): //todo:method
                    break;

                case bool _ when type == typeof(DtoInstance):
                    SimpleCommand.ExecuteNonQuery(connStr, $@"delete InstanceData where instanceid={id}");
                    SimpleCommand.ExecuteNonQuery(connStr, $@"delete Instance where id={id}");
                    break;

                case bool _ when type == typeof(DtoTask):
                    SimpleCommand.ExecuteNonQuery(connStr,
                        $@"delete InstanceData where instanceid in (select id from instance where TaskID={id})");
                    SimpleCommand.ExecuteNonQuery(connStr, $@"delete Instance where TaskID={id}");
                    SimpleCommand.ExecuteNonQuery(connStr, $@"delete Task where id={id}");
                    break;
            }
        }


        public void CreateBase(string baseConnStr)
        {
            // TODO: check db exists ~find way to cut redundant code 
            // TODO: refactoring
            // Task table refac (new base-creating uses)
            // 4. ConnStr => DataSource table

            SimpleCommand.ExecuteNonQuery(baseConnStr, @"
                IF OBJECT_ID('ExporterConfig') IS NULL
                CREATE TABLE ExporterConfig
                (Id INT PRIMARY KEY IDENTITY,
                ExporterType NVARCHAR(255) NOT NULL,
                JsonConfig NVARCHAR(4000) NOT NULL); ");

            SimpleCommand.ExecuteNonQuery(baseConnStr, @"
                IF OBJECT_ID('RecepientGroup') IS NULL
                CREATE TABLE RecepientGroup
                Name NVARCHAR(127) NOT NULL,
                Addresses NVARCHAR(4000) NOT NULL,
                AddressesBcc NVARCHAR(4000) NULL
                ); ");

            var existScheduleTable = Convert.ToInt64(SimpleCommand
                .ExecuteQueryFirstColumn<object>(baseConnStr, @"
               SELECT ISNULL(OBJECT_ID('Schedule'),0)")
                .First());
            if (existScheduleTable == 0)
            {
                var schedules = new[]
                {
                    new DtoSchedule {Name = "workDaysEvening", Schedule = "30 22 * * 1-5"},
                    new DtoSchedule {Name = "sundayEvening", Schedule   = "30 22 * * 0"}
                };
                SimpleCommand.ExecuteNonQuery(baseConnStr, @"
                CREATE TABLE Schedule
                (Id INT PRIMARY KEY IDENTITY,
                Name NVARCHAR(127) NOT NULL,
                Schedule NVARCHAR(255) NOT NULL
                )");
                schedules.WriteToServer(baseConnStr, "Schedule");
            }

            SimpleCommand.ExecuteNonQuery(baseConnStr, @"
                IF OBJECT_ID('Report') IS NULL
                CREATE TABLE Report
                (Id INT PRIMARY KEY IDENTITY,
                Name NVARCHAR(127) NOT NULL,
                ConnectionString NVARCHAR(255) NULL,
                ViewTemplate NVARCHAR(MAX) NOT NULL,
                Query NVARCHAR(MAX) NOT NULL,
                ReportType TINYINT NOT NULL,
                QueryTimeOut SMALLINT NOT NULL
                ); ");

            SimpleCommand.ExecuteNonQuery(baseConnStr, @"
                IF OBJECT_ID('TelegramChannel') IS NULL
                CREATE TABLE TelegramChannel
                (Id INT PRIMARY KEY IDENTITY,
                Name NVARCHAR(127) NOT NULL,
                Description NVARCHAR(255) NULL,
                ChatId BIGINT NOT NULL,
                Type TINYINT NOT NULL
                );  ");

            SimpleCommand.ExecuteNonQuery(baseConnStr, @"
                IF OBJECT_ID('Task') IS NULL
                CREATE TABLE Task
                (Id INT PRIMARY KEY IDENTITY,
                ReportId INT NOT NULL,
                ScheduleId INT NULL,
                TryCount TINYINT NOT NULL,
                CONSTRAINT FK_Task_Schedule FOREIGN KEY(ScheduleId) 
                REFERENCES Schedule(Id),
                CONSTRAINT FK_Task_Report FOREIGN KEY(ReportId) 
                REFERENCES Report(Id)
                )");

            SimpleCommand.ExecuteNonQuery(baseConnStr, @"
                IF OBJECT_ID('ExporterToTaskBinder') IS NULL
                CREATE TABLE ExporterToTaskBinder
                (Id INT PRIMARY KEY IDENTITY,
                TaskId INT NOT NULL,
                ConfigId INT NOT NULL,
                CONSTRAINT FK_ExporterToTaskBinder_Task FOREIGN KEY(TaskId) 
                REFERENCES Task(Id),
                CONSTRAINT FK_ExporterToTaskBinder_ExporterConfig FOREIGN KEY(ConfigId) 
                REFERENCES ExporterConfig(Id)); ");

            SimpleCommand.ExecuteNonQuery(baseConnStr, @"
                IF OBJECT_ID('Instance') IS NULL
                CREATE TABLE Instance
                (Id INT PRIMARY KEY IDENTITY,
                TaskID INT NOT NULL,
                StartTime DATETIME NOT NULL,
                Duration INT NOT NULL,
                State INT NOT NULL,
                TryNumber INT NOT NULL,
                CONSTRAINT FK_Instance_Task FOREIGN KEY(TaskID)
                REFERENCES Task(Id)
                )");

            SimpleCommand.ExecuteNonQuery(baseConnStr, @"
                IF object_id('InstanceData') IS NULL
                CREATE TABLE InstanceData(
	            InstanceId INT NOT NULL,
	            Data VARBINARY(MAX) NULL,
	            ViewData VARBINARY(MAX) NULL,
                CONSTRAINT FK_InstanceData_Data FOREIGN KEY(InstanceId)
                REFERENCES Instance(Id)
                )");
        }
    } //class
}
