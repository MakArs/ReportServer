using System;
using System.Collections.Generic;
using System.Linq;
using Gerakul.FastSql.Common;
using Gerakul.FastSql.SqlServer;
using Monik.Common;
using ReportService.Entities;
using ReportService.Interfaces.Core;

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

        public object GetBaseQueryResult(string query)
        {
            object result = context
                    .CreateSimple(new QueryOptions(30), query)
                    .ExecuteQueryFirstColumn<object>().ToList().First();
           
            return result;
        }
        
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
                               $"{e.Message}");
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
                               $"{e.Message}");
                throw;
            }
        }

        public DtoOperInstance GetFullOperInstanceById(int operInstanceId)
        {
            try
            {
                return context.CreateSimple
                        ("select oi.id,TaskInstanceId,OperationId,StartTime,Duration,State,DataSet,ErrorMessage,Name as OperName " +
                         "from OperInstance oi join operation op on oi.OperationId=op.Id " +
                         $"where oi.id={operInstanceId}")
                    .ExecuteQuery<DtoOperInstance>()
                    .ToList().First();
            }
            catch (Exception e)
            {
                SendAppWarning("Error occured while getting operation instance data: " +
                               $"{e.Message}");
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
                SendAppWarning("Error occured while updating " +
                               $"{tableName} record: {e.Message}");
            }
        }

        public void UpdateTask(DtoTask task, params DtoOperation[] bindedOpers)
        {
            context.UsingTransaction(transContext =>
            {
                try
                {
                    var currentOperIds = context.CreateSimple
                            ($"select id from operation where taskid={task.Id}")
                        .ExecuteQueryFirstColumn<int>().ToList();

                    var newOperIds = bindedOpers.Select(oper => oper.Id).ToList();

                    var operIdsToDelete = currentOperIds.Except(newOperIds)
                        .ToList();

                    var opersToUpdate = bindedOpers.Where(oper =>
                        newOperIds.Intersect(currentOperIds).Contains(oper.Id)).ToList();

                    var opersToWrite = bindedOpers.Where(oper =>
                        newOperIds.Except(currentOperIds).Contains(oper.Id));

                    transContext.Update("Task", task, "Id");

                    if (operIdsToDelete.Any())
                        transContext.CreateSimple(
                                $"Update Operation set isDeleted=1 where TaskId={task.Id} and " +
                                $"id in ({string.Join(",", operIdsToDelete)})")
                            .ExecuteNonQuery();

                    foreach (var oper in opersToUpdate
                    ) //no chances of so many  opers will be updated so no losses
                        transContext.Update("Operation", oper, "Id");


                    opersToWrite.WriteToServer(transContext, "Operation");
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

        public List<int> UpdateOperInstancesAndGetIds()
        {
            var ids=context.CreateSimple(@"UPDATE OperInstance
            SET state=3,ErrorMessage='Unknown error.The service was probably stopped during the task execution.'
            OUTPUT INSERTED.id
            WHERE state=1").ExecuteQueryFirstColumn<int>().ToList();

            return ids;
        }

        public List<int> UpdateTaskInstancesAndGetIds()
        {
            var ids = context.CreateSimple(@"UPDATE TaskInstance
            SET state=3
            OUTPUT INSERTED.id
            WHERE state=1").ExecuteQueryFirstColumn<int>().ToList();

            return ids;
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
                ScheduleId INT NULL,
                Parameters NVARCHAR(1024) NULL
                CONSTRAINT FK_Task_Schedule FOREIGN KEY(ScheduleId) 
                REFERENCES Schedule(Id)
                )")
                .ExecuteNonQuery();

            createBaseContext.CreateSimple(@"
                IF OBJECT_ID('Operation') IS NULL
                CREATE TABLE Operation
                (Id BIGINT PRIMARY KEY IDENTITY,
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
                (Id BIGINT PRIMARY KEY IDENTITY,
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
                Id BIGINT PRIMARY KEY IDENTITY,
	            TaskInstanceId BIGINT NOT NULL,
                OperationId BIGINT NOT NULL,
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