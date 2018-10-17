using System;
using System.Collections.Generic;
using System.Linq;
using Gerakul.FastSql.Common;
using Gerakul.FastSql.SqlServer;
using Monik.Common;
using ReportService.Interfaces;

namespace ReportService.Core
{
    public class Repository : IRepository
    {
        private readonly IMonik monik;
        private readonly ConnectionStringContext context;

        public Repository(string connStr, IMonik monik)
        {
            context = SqlContextProvider.DefaultInstance.CreateContext(connStr);
            this.monik = monik;
        } //ctor

        public List<DtoTaskInstance> GetInstancesByTaskId(int taskId)
        {
            try
            {
                return context.CreateSimple($"select * from TaskInstance where TaskId={taskId}")
                    .ExecuteQuery<DtoTaskInstance>().ToList();
            }
            catch (Exception e)
            {
                SendAppWarning("Error occured while getting task instances: " +
                               $"({e.Message})");
                throw;
            }
        }


        public List<DtoOperInstance> GetOperInstancesByTaskInstanceId(int taskInstanceId)
        {
            try
            {
                return context.CreateSimple
                    ("select Id,TaskInstanceId,OperationId,StartTime,Duration,State,null as DataSet," +
                     $"null as ErrorMessage from OperInstance where TaskInstanceId={taskInstanceId}")
                    .ExecuteQuery<DtoOperInstance>().ToList();
            }
            catch (Exception e)
            {
                SendAppWarning("Error occured while getting operation instances: " +
                               $"({e.Message})");
                throw;
            }
        }

        public DtoOperInstance GetFullOperInstanceById(int operInstanceId)
        {
            try
            {
                return context.CreateSimple
                        ($"select * from OperInstance where Id={operInstanceId}")
                    .ExecuteQuery<DtoOperInstance>()
                    .ToList().First();
            }
            catch (Exception e)
            {
                SendAppWarning("Error occured while getting operation instance data: " +
                               $"({e.Message})");
                throw;
            }
        }

        public List<T> GetListEntitiesByDtoType<T>() where T : IDtoEntity, new()
        {
            var tableName = typeof(T).Name.Remove(0, 3);
            try
            {
                return context.CreateSimple($"select * from {tableName}")
                    .ExecuteQuery<T>()
                    .ToList();
            }
            catch (Exception e)
            {
                SendAppWarning("Error occured while getting " +
                               $"{tableName} list: {e.Message}");
                return null;
            }
        }

        public int CreateEntity<T>(T entity) where T : IDtoEntity
        {
            var tableName = typeof(T).Name.Remove(0, 3);

            try
            {
                return context.CreateInsertWithOutput($"{tableName}", entity,
                        new List<string> {"Id"}, "Id")
                    .ExecuteQueryFirstColumn<int>().First();
            }

            catch (Exception e)
            {
                SendAppWarning("Error occured while creating new " +
                               $"{tableName} record: {e.Message}");
                return 0;
            }
        }

        public int CreateTask(DtoTask task, params DtoOperation[] bindedOpers)
        {
            int newTaskId = 0;
            context.UsingTransaction(transContext =>
            {
                try
                {
                    newTaskId = transContext.CreateInsertWithOutput("Task", task,
                            new List<string> {"Id"}, "Id")
                        .ExecuteQueryFirstColumn<int>().First();

                    if (bindedOpers == null)
                        return;

                    foreach (var oper in bindedOpers)
                    {
                        oper.TaskId = newTaskId;
                    }

                    bindedOpers.WriteToServer(transContext, "Operation");
                }

                catch (Exception e)
                {
                    SendAppWarning("Error occured while creating new Task" +
                                   $" record: {e.Message}");
                    throw;
                }
            });

            return newTaskId;
        }

        public void UpdateEntity<T>(T entity) where T : IDtoEntity
        {
            var tableName = typeof(T).Name.Remove(0, 3);

            try
            {
                context.Update($"{tableName}", entity, "Id");
            }

            catch (Exception e)
            {
                SendAppWarning("Error occured while updating прои" +
                               $"{tableName} record: {e.Message}");
            }
        }

        public void UpdateTask(DtoTask task, params DtoOperation[] bindedOpers)
        {
            context.UsingTransaction(transContext =>
            {
                try
                {
                    transContext.Update("Task", task, "Id");

                    transContext.CreateSimple($"Delete Operation where TaskId={task.Id}")
                        .ExecuteNonQuery();

                    bindedOpers.WriteToServer(transContext, "Operation");
                }

                catch (Exception e)
                {
                    SendAppWarning("Error occured while updating Task" +
                                   $" record: {e.Message}");
                    throw;
                }
            });
        }

        public void DeleteEntity<T>(int id) where T : IDtoEntity
        {
            var type = typeof(T);
            var tableName = type.Name.Remove(0, 3);

            switch (true)
            {
                case bool _ when type == typeof(DtoTaskInstance):
                    context.UsingTransaction(transContext =>
                    {
                        try
                        {
                            transContext
                                .CreateCommand($"delete OperInstance where TaskInstanceId={id}")
                                .ExecuteNonQuery();
                            transContext.CreateCommand($"delete TaskInstance where id={id}")
                                .ExecuteNonQuery();
                        }

                        catch (Exception e)
                        {
                            SendAppWarning("Error occured while deleting Task instance" +
                                           $" record: {e.Message}");
                            throw;
                        }
                    });
                    break;

                case bool _ when type == typeof(DtoTask):
                    context.UsingTransaction(transContext =>
                    {
                        try
                        {
                            transContext.CreateCommand(
                                    $@"delete OperInstance where TaskInstanceId in
                                            (select id from TaskInstance where TaskId={id})")
                                .ExecuteNonQuery();
                            transContext.CreateCommand($"delete TaskInstance where TaskID={id}")
                                .ExecuteNonQuery();
                            transContext.CreateCommand($"delete Operation where TaskID={id}")
                                .ExecuteNonQuery();
                            transContext.CreateCommand($"delete Task where id={id}")
                                .ExecuteNonQuery();
                        }
                        catch (Exception e)
                        {
                            SendAppWarning("Error occured while deleting Task" +
                                           $" record: {e.Message}");
                            throw;
                        }
                    });
                    break;

                case bool _ when type == typeof(DtoOperTemplate):
                    context.UsingTransaction(transContext =>
                    {
                        try
                        {
                            context.CreateSimple(
                                    $"update {tableName} set isdeleted=1 where Id={id}")
                                .ExecuteNonQuery();
                        }

                        catch (Exception e)
                        {
                            SendAppWarning("Error occured while deleting operation template" +
                                           $" template instance record: {e.Message}");
                            throw;
                        }
                    });
                    break;

                default:
                    try
                    {
                        context.CreateSimple($"delete {tableName} where Id={id}").ExecuteNonQuery();
                    }

                    catch (Exception e)
                    {
                        SendAppWarning($"Error occured while deleting {tableName}" +
                                       $" record: {e.Message}");
                        throw;
                    }

                    break;
            }

            //case bool _ when type == typeof(DtoOperation): //todo:do we really need this method?
            //break;

            //case bool _ when type == typeof(DtoTelegramChannel): //todo:method
            //break;

            //case bool _ when type == typeof(DtoRecepientGroup): //todo:method
            //try
            //{
            //    SimpleCommand.ExecuteNonQuery(connStr,
            //        $@"delete RecepientGroup where Id={id}");
            //}
        }

        private void SendAppWarning(string msg)
        {
            monik.ApplicationWarning(msg);
            Console.WriteLine(msg);
        }

        public void CreateBase(string baseConnStr)
        {
            var createBaseContext = SqlContextProvider.DefaultInstance
                .CreateContext(baseConnStr);
            // TODO: check db exists ~find way to cut redundant code 
            createBaseContext.CreateSimple(@"
                IF OBJECT_ID('OperTemplate') IS NULL
                CREATE TABLE OperTemplate
                (Id INT PRIMARY KEY IDENTITY,
                ImplementationType NVARCHAR(255) NOT NULL,
                Name NVARCHAR(255) NOT NULL,
                ConfigTemplate NVARCHAR(MAX) NOT NULL);")
                .ExecuteNonQuery();

            createBaseContext.CreateSimple(@"
                IF OBJECT_ID('RecepientGroup') IS NULL
                CREATE TABLE RecepientGroup
                (Name NVARCHAR(127) NOT NULL,
                Addresses NVARCHAR(4000) NOT NULL,
                AddressesBcc NVARCHAR(4000) NULL
                ); ")
                .ExecuteNonQuery();

            createBaseContext.CreateSimple(@"
                IF OBJECT_ID('TelegramChannel') IS NULL
                CREATE TABLE TelegramChannel
                (Id INT PRIMARY KEY IDENTITY,
                Name NVARCHAR(127) NOT NULL,
                Description NVARCHAR(255) NULL,
                ChatId BIGINT NOT NULL,
                Type TINYINT NOT NULL
                );  ")
                .ExecuteNonQuery();

            var existScheduleTable = Convert.ToInt64(createBaseContext
                .CreateSimple(@"
               SELECT ISNULL(OBJECT_ID('Schedule'),0)")
                .ExecuteQueryFirstColumn<object>()
                .First());

            if (existScheduleTable == 0)
            {
                var schedules = new[]
                {
                    new DtoSchedule {Name = "workDaysEvening", Schedule = "30 22 * * 1-5"},
                    new DtoSchedule {Name = "sundayEvening", Schedule = "30 22 * * 0"}
                };

                createBaseContext.CreateSimple(@"
                CREATE TABLE Schedule
                (Id INT PRIMARY KEY IDENTITY,
                Name NVARCHAR(127) NOT NULL,
                Schedule NVARCHAR(255) NOT NULL
                )")
                    .ExecuteNonQuery();
                schedules.WriteToServer(createBaseContext, "Schedule");
            }

            createBaseContext.CreateSimple(@"
                IF OBJECT_ID('Task') IS NULL
                CREATE TABLE Task
                (Id INT PRIMARY KEY IDENTITY,
                Name NVARCHAR(127) NOT NULL,
                ScheduleId INT NULL
                CONSTRAINT FK_Task_Schedule FOREIGN KEY(ScheduleId) 
                REFERENCES Schedule(Id)
                )")
                .ExecuteNonQuery();

            createBaseContext.CreateSimple(@"
                IF OBJECT_ID('Operation') IS NULL
                CREATE TABLE Operation
                (Id INT PRIMARY KEY IDENTITY,
                TaskId INT NOT NULL,
                Number TINYINT NOT NULL,
                Name NVARCHAR(255) NOT NULL,
                ImplementationType NVARCHAR(255) NOT NULL,
                IsDefault BIT NOT NULL DEFAULT 0,
                Config NVARCHAR(MAX) NOT NULL,
                IsDeleted BIT NOT NULL DEFAULT 0,
                CONSTRAINT FK_Operation_Task FOREIGN KEY(TaskId) 
                REFERENCES Task(Id));")
                .ExecuteNonQuery();

            createBaseContext.CreateSimple(@"
                IF OBJECT_ID('TaskInstance') IS NULL
                CREATE TABLE TaskInstance
                (Id INT PRIMARY KEY IDENTITY,
                TaskID INT NOT NULL,
                StartTime DATETIME NOT NULL,
                Duration INT NOT NULL,
                State INT NOT NULL,
                CONSTRAINT FK_TaskInstance_Task FOREIGN KEY(TaskID)
                REFERENCES Task(Id)
                )")
                .ExecuteNonQuery();

            createBaseContext.CreateSimple(@"
                IF object_id('OperInstance') IS NULL
                CREATE TABLE OperInstance(
                Id INT PRIMARY KEY IDENTITY,
	            TaskInstanceId INT NOT NULL,
                OperationId INT NOT NULL,
                StartTime DATETIME NOT NULL,
                Duration INT NOT NULL,
                State INT NOT NULL,
	            DataSet VARBINARY(MAX) NULL,
                ErrorMessage NVARCHAR(511) NULL,
                CONSTRAINT FK_OperInstance_Operation FOREIGN KEY(OperationId) 
                REFERENCES Operation(Id),
                CONSTRAINT FK_OperInstance_TaskInstance FOREIGN KEY(TaskInstanceId)
                REFERENCES TaskInstance(Id)
                )")
                .ExecuteNonQuery();
        } //database structure creating
    } //class
}