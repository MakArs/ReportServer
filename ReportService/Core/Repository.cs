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

        public List<DtoTaskInstance> GetInstancesByTaskId(int taskId)
        {
            return SimpleCommand.ExecuteQuery<DtoTaskInstance>(connStr,
                    $"select * from TaskInstance where TaskId={taskId}")
                .ToList();
        }

        public List<DtoOperInstance> GetOperInstancesByTaskInstanceId(int taskInstanceId)
        {
            return SimpleCommand.ExecuteQuery<DtoOperInstance>(connStr,
                    $"select Id,TaskInstanceId,OperId,ErrorMessage from OperInstance where TaskInstanceId={taskInstanceId}")
                .ToList();
        }

        public DtoOperInstance GetFullOperInstanceById(int operInstanceId)
        {
            return SimpleCommand.ExecuteQuery<DtoOperInstance>(connStr,
                    $@"select * from OperInstance where Id={operInstanceId}")
                .ToList().First();
        }

        public List<T> GetListEntitiesByDtoType<T>() where T : IDtoEntity, new()
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

        public int CreateEntity<T>(T entity) where T : IDtoEntity
        {
            var tableName = typeof(T).Name.Remove(0, 3);

            try
            {
                return (int) MappedCommand.InsertAndGetId(connStr, $"{tableName}", entity, "Id");
            }

            catch (Exception e)
            {
                Console.WriteLine(e);
                return 0;
            }
        }

        public void UpdateEntity<T>(T entity) where T : IDtoEntity
        {
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
                case bool _ when type == typeof(DtoOper): //todo:method
                    break;

                case bool _ when type == typeof(DtoSchedule): //todo:method
                    break;

                case bool _ when type == typeof(DtoTaskOper): //todo:method
                    break;

                case bool _ when type == typeof(DtoTaskOper): //todo:method
                    break;

                case bool _ when type == typeof(DtoTelegramChannel): //todo:method
                    break;

                case bool _ when type == typeof(DtoRecepientGroup): //todo:method
                    break;

                case bool _ when type == typeof(DtoTaskInstance):
                    SimpleCommand.ExecuteNonQuery(connStr,
                        $@"delete OperInstance where TaskInstanceId={id}");
                    SimpleCommand.ExecuteNonQuery(connStr, $@"delete TaskInstance where id={id}");
                    break;

                case bool _ when type == typeof(DtoTask):
                    SimpleCommand.ExecuteNonQuery(connStr,
                        $@"delete OperInstance where TaskInstanceId in (select id from TaskInstance where TaskId={id})");
                    SimpleCommand.ExecuteNonQuery(connStr,
                        $@"delete TaskInstance where TaskID={id}");
                    SimpleCommand.ExecuteNonQuery(connStr, $@"delete Task where id={id}");
                    break;
            }
        }

        public void CreateBase(string baseConnStr)
        {
            // TODO: check db exists ~find way to cut redundant code 

            SimpleCommand.ExecuteNonQuery(baseConnStr, @"
                IF OBJECT_ID('Oper') IS NULL
                CREATE TABLE Oper
                (Id INT PRIMARY KEY IDENTITY,
                Type NVARCHAR(255) NOT NULL,
                Name NVARCHAR(255) NOT NULL,
                Config NVARCHAR(MAX) NOT NULL);");

            SimpleCommand.ExecuteNonQuery(baseConnStr, @"
                IF OBJECT_ID('RecepientGroup') IS NULL
                CREATE TABLE RecepientGroup
                (Name NVARCHAR(127) NOT NULL,
                Addresses NVARCHAR(4000) NOT NULL,
                AddressesBcc NVARCHAR(4000) NULL
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

            var existScheduleTable = Convert.ToInt64(SimpleCommand
                .ExecuteQueryFirstColumn<object>(baseConnStr, @"
               SELECT ISNULL(OBJECT_ID('Schedule'),0)")
                .First());
            if (existScheduleTable == 0)
            {
                var schedules = new[]
                {
                    new DtoSchedule {Name = "workDaysEvening", Schedule = "30 22 * * 1-5"},
                    new DtoSchedule {Name = "sundayEvening", Schedule = "30 22 * * 0"}
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
                IF OBJECT_ID('Task') IS NULL
                CREATE TABLE Task
                (Id INT PRIMARY KEY IDENTITY,
                Name NVARCHAR(127) NOT NULL,
                ScheduleId INT NULL
                CONSTRAINT FK_Task_Schedule FOREIGN KEY(ScheduleId) 
                REFERENCES Schedule(Id)
                )");

            SimpleCommand.ExecuteNonQuery(baseConnStr, @"
                IF OBJECT_ID('TaskOper') IS NULL
                CREATE TABLE TaskOper
                (Id INT PRIMARY KEY IDENTITY,
                TaskId INT NOT NULL,
                OperId INT NOT NULL,
                Number TINYINT NOT NULL,
                CONSTRAINT FK_TaskOper_Task FOREIGN KEY(TaskId) 
                REFERENCES Task(Id),
                CONSTRAINT FK_TaskOper_Oper FOREIGN KEY(OperId) 
                REFERENCES Oper(Id));");

            SimpleCommand.ExecuteNonQuery(baseConnStr, @"
                IF OBJECT_ID('TaskInstance') IS NULL
                CREATE TABLE TaskInstance
                (Id INT PRIMARY KEY IDENTITY,
                TaskID INT NOT NULL,
                StartTime DATETIME NOT NULL,
                Duration INT NOT NULL,
                State INT NOT NULL,
                CONSTRAINT FK_TaskInstance_Task FOREIGN KEY(TaskID)
                REFERENCES Task(Id)
                )");

            SimpleCommand.ExecuteNonQuery(baseConnStr, @"
                IF object_id('OperInstance') IS NULL
                CREATE TABLE OperInstance(
                Id INT PRIMARY KEY IDENTITY,
	            TaskInstanceId INT NOT NULL,
                OperId INT NOT NULL,
                StartTime DATETIME NOT NULL,
                Duration INT NOT NULL,
                State INT NOT NULL,
	            DataSet VARBINARY(MAX) NULL,
                ErrorMessage NVARCHAR(511) NULL,
                CONSTRAINT FK_OperInstance_Oper FOREIGN KEY(OperId) 
                REFERENCES Oper(Id),
                CONSTRAINT FK_OperInstance_TaskInstance FOREIGN KEY(TaskInstanceId)
                REFERENCES TaskInstance(Id)
                )");
        }
    } //class
}
