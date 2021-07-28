using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Dapper;
using Dapper.Contrib.Extensions;
using Monik.Common;
using ReportService.Entities;
using ReportService.Entities.Dto;
using ReportService.Interfaces.Core;

namespace ReportService.Core
{
    public class SqlServerRepository : IRepository
    {
        private readonly IMonik monik;
        private readonly string connectionString;

        public SqlServerRepository(string connStr, IMonik monik)
        {
            connectionString = connStr;

            this.monik = monik;
        }

        public async Task<object> GetBaseQueryResult(string query, CancellationToken token)
        {

            await using var connection = new SqlConnection(connectionString);

            dynamic result =
                await connection.QueryFirstAsync<dynamic>(new CommandDefinition(query,
                    commandTimeout: 30, cancellationToken: token));

            var value = (result as IDictionary<string, object>).First().Value;
            return value;
        }

        public async Task<List<DtoTaskInstance>> GetAllTaskInstances(long taskId)
        {
            await using var connection = new SqlConnection(connectionString);

            try
            {
                return (await connection.QueryAsync<DtoTaskInstance>(
                        $"select * from TaskInstance with(nolock) where TaskId={taskId}"))
                    .ToList();
            }

            catch (Exception e)
            {
                SendAppWarning("Error occured while getting task instances: " +
                               $"{e.Message}");
                throw;
            }
        }

        public TaskState GetTaskStateById(long taskId)
        {
            using var connection = new SqlConnection(connectionString);

            try
            {
                return connection.QueryFirst<TaskState>(
                    $@"SELECT max(case when State=2 then dateadd(ms,ti.Duration,ti.[StartTime]) else null end) LastSuccessfulFinish,
	                                        count(case when State=1 then 1 else null end) InProcessCount,
                                            iif(max(ti.[StartTime])>max(t.[UpdateDateTime]),
                                                    max([StartTime]),
                                                    max(t.[UpdateDateTime])) LastStart
                                            FROM
											[dbo].[Task] t with(nolock)
											left join [dbo].[TaskInstance] ti with(nolock)
											on t.id=ti.TaskID
											where t.id={taskId}",
                    commandTimeout: 30);
            }

            catch (Exception e)
            {
                SendAppWarning("Error occured while getting task last time: " +
                               $"{e.Message}");
                throw;
            }
        }

        public List<DtoOperInstance> GetTaskOperInstances(long taskInstanceId)
        {
            using var connection = new SqlConnection(connectionString);

            try
            {
                return connection.Query<DtoOperInstance>
                    ($@"select Id,TaskInstanceId,OperationId,StartTime,Duration,State,null as DataSet,
                        null as ErrorMessage from OperInstance with(nolock) where TaskInstanceId={taskInstanceId}",
                        commandTimeout: 60)
                    .ToList();
            }

            catch (Exception e)
            {
                SendAppWarning("Error occured while getting operation instances: " +
                               $"{e.Message}");
                throw;
            }
        }

        public List<DtoOperInstance> GetFullTaskOperInstances(long taskInstanceId)
        {
            using var connection = new SqlConnection(connectionString);

            try
            {
                return connection.Query<DtoOperInstance>
                    ($@"select Id,TaskInstanceId,OperationId,StartTime,Duration,State,DataSet,
                        ErrorMessage from OperInstance with(nolock) where TaskInstanceId={taskInstanceId}",
                        commandTimeout: 60)
                    .ToList();
            }

            catch (Exception e)
            {
                SendAppWarning("Error occured while getting operation instances: " +
                               $"{e.Message}");
                throw;
            }
        }

        public DtoOperInstance GetFullOperInstanceById(long operInstanceId)
        {
            using var connection = new SqlConnection(connectionString);

            try
            {
                return connection.QueryFirst<DtoOperInstance>
                ($@"select oi.id,TaskInstanceId,OperationId,StartTime,Duration,State,DataSet,ErrorMessage,Name as OperName
                    from OperInstance oi with(nolock)
                    join operation op with(nolock)
                    on oi.OperationId=op.Id
                    where oi.id={operInstanceId}",
                    commandTimeout: 60);
            }

            catch (Exception e)
            {
                SendAppWarning("Error occured while getting operation instance data: " +
                               $"{e.Message}");
                throw;
            }
        }

        public List<T> GetListEntitiesByDtoType<T>() where T : class, IDtoEntity
        {
            using var connection = new SqlConnection(connectionString);

            var tableName = typeof(T).Name.Remove(0, 3);

            try
            {
                return connection.GetAll<T>().ToList();
            }

            catch (Exception e)
            {
                SendAppWarning("Error occured while getting " +
                               $"{tableName} list: {e.Message}");
                return null;
            }
        }

        public long CreateEntity<T>(T entity) where T : class, IDtoEntity
        {
            using var connection = new SqlConnection(connectionString);

            var tableName = typeof(T).Name.Remove(0, 3);

            try
            {
                return connection.Insert(entity,
                    commandTimeout: 60);
            }

            catch (Exception e)
            {
                SendAppWarning("Error occured while creating new " +
                               $"{tableName} record: {e.Message}");
                return default;
            }
        }

        public long CreateTask(DtoTask task, params DtoOperation[] bindedOpers)
        {
            using var connection = new SqlConnection(connectionString);

            long newTaskId;

            connection.Open();

            using (var transaction = connection.BeginTransaction())
            {
                try
                {
                    newTaskId = connection.Insert(task,
                        commandTimeout: 60, transaction: transaction);

                    if (bindedOpers != null)

                        foreach (var oper in bindedOpers)
                        {
                            oper.TaskId = newTaskId;
                        }

                    connection.Insert(bindedOpers,
                        commandTimeout: 60, transaction: transaction);

                    transaction.Commit();
                }

                catch (Exception e)
                {
                    transaction.Rollback();

                    SendAppWarning("Error occured while creating new Task" +
                                   $" record: {e.Message}");
                    throw;
                }
            }

            return newTaskId;
        }

        public void UpdateEntity<T>(T entity) where T : class, IDtoEntity
        {
            using var connection = new SqlConnection(connectionString);

            var tableName = typeof(T).Name.Remove(0, 3);

            try
            {
                connection.Update(entity, commandTimeout: 60);
            }

            catch (Exception e)
            {
                SendAppWarning("Error occured while updating " +
                               $"{tableName} record: {e.Message}");
            }
        }

        public void UpdateTask(DtoTask task, params DtoOperation[] bindedOpers)
        {
            using var connection = new SqlConnection(connectionString);

            var currentOperIds = connection.Query<long>
            ($@"select id from operation with(nolock) where taskid={task.Id}
                and isDeleted=0",
                commandTimeout: 60);

            connection.Open();

            using var transaction = connection.BeginTransaction();
            try
            {
                var newOperIds = bindedOpers.Select(oper => oper.Id);

                var operIdsToDelete = currentOperIds.Except(newOperIds);

                var opersToUpdate = bindedOpers.Where(oper =>
                    newOperIds.Intersect(currentOperIds).Contains(oper.Id));

                var opersToWrite = bindedOpers.Where(oper =>
                    newOperIds.Except(currentOperIds).Contains(oper.Id));

                connection.Update(task, commandTimeout: 60, transaction: transaction);

                if (operIdsToDelete.Any())
                    connection.Execute(
                        $@"Update Operation set isDeleted=1 where TaskId={task.Id} and
                            id in ({string.Join(",", operIdsToDelete)})",
                        commandTimeout: 60, transaction: transaction);

                connection.Update(opersToUpdate, commandTimeout: 60, transaction: transaction);

                connection.Insert(opersToWrite, commandTimeout: 60, transaction: transaction);

                transaction.Commit();
            }

            catch (Exception e)
            {
                transaction.Rollback();

                SendAppWarning("Error occured while updating Task" +
                               $" record: {e.Message}");
                throw;
            }
        }

        public void DeleteEntity<T, TKey>(TKey id) where T : IDtoEntity
        {
            using var connection = new SqlConnection(connectionString);

            var type = typeof(T);
            var tableName = type.Name.Remove(0, 3);

            connection.Open();

            switch (true)
            {
                case { } when type == typeof(DtoTaskInstance):
                    using (var transaction = connection.BeginTransaction())
                    {
                        try
                        {
                            connection.Execute($"delete OperInstance where TaskInstanceId={id}",
                                commandTimeout: 60, transaction: transaction);

                            connection.Execute($"delete TaskInstance where id={id}",
                                commandTimeout: 60, transaction: transaction);

                            transaction.Commit();
                        }

                        catch (Exception e)
                        {
                            SendAppWarning("Error occured while deleting Task instance" +
                                           $" record: {e.Message}");
                            transaction.Rollback();
                            throw;
                        }
                    }

                    break;

                case { } when type == typeof(DtoTask):
                    using (var transaction = connection.BeginTransaction())
                    {
                        try
                        {

                            connection.Execute($@"delete OperInstance where TaskInstanceId in
                                            (select id from TaskInstance with(nolock) where TaskId={id})",
                                commandTimeout: 60, transaction: transaction);

                            connection.Execute($"delete TaskInstance where TaskID={id}",
                                commandTimeout: 60, transaction: transaction);

                            connection.Execute($"delete Operation where TaskID={id}",
                                commandTimeout: 60, transaction: transaction);

                            connection.Execute($"delete Task where id={id}",
                                commandTimeout: 60, transaction: transaction);

                            transaction.Commit();
                        }
                        catch (Exception e)
                        {
                            transaction.Rollback();

                            SendAppWarning("Error occured while deleting Task" +
                                           $" record: {e.Message}");
                            throw;
                        }
                    }

                    break;

                default:
                    try
                    {
                        connection.Execute($"delete {tableName} where Id={id}",
                            commandTimeout: 60);
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

        public List<long> UpdateOperInstancesAndGetIds()
        {
            using var connection = new SqlConnection(connectionString);

            var ids = connection.Query<long>(@"UPDATE OperInstance
            SET state=3,ErrorMessage='Unknown error.The service was probably stopped during the task execution.'
            OUTPUT INSERTED.id
            WHERE state=1").ToList();

            return ids;
        }

        public List<long> UpdateTaskInstancesAndGetIds()
        {
            using var connection = new SqlConnection(connectionString);

            var ids = connection.Query<long>(@"UPDATE TaskInstance
            SET state=3
            OUTPUT INSERTED.id
            WHERE state=1").ToList();

            return ids;
        }

        private void SendAppWarning(string msg)
        {
            monik.ApplicationWarning(msg);
            Console.WriteLine(msg);
        }

        public long CreateTaskRequestInfo(TaskRequestInfo taskRequestInfo)
        {
            using var connection = new SqlConnection(connectionString);

            long newTaskRequestInfoId;

            connection.Open();

            using (var transaction = connection.BeginTransaction())
            {
                try
                {
                    newTaskRequestInfoId = connection.Insert(taskRequestInfo,
                        commandTimeout: 60, transaction: transaction);

                    transaction.Commit();
                }

                catch (Exception e)
                {
                    transaction.Rollback();
                    throw;
                }
            }

            return newTaskRequestInfoId;
        }

        public TaskRequestInfo GetTaskRequestInfoById(long taskRequestId)
        {
            using var connection = new SqlConnection(connectionString);

            try
            {
                return connection.QueryFirst<TaskRequestInfo>
                ($@"
                select
                    tri.RequestId,
                    tri.TaskId,
                    tri.Parameters,
                    tri.TaskInstanceId,
                    tri.CreateTime,
                    tri.UpdateTime,
                    tri.Status
                from TaskRequestInfo tri with(nolock)
                where tri.RequestId = {taskRequestId}",
                    commandTimeout: 60);
            }

            catch (Exception e)
            {
                SendAppWarning("Error occured while getting task request info data: " +
                               $"{e.Message}");
                throw;
            }
        }

        public void CreateBase(string baseConnStr)
        {
            using var connection = new SqlConnection(baseConnStr);

            // TODO: check db exists ~find way to cut redundant code 
            connection.Execute(@"
                IF OBJECT_ID('OperTemplate') IS NULL
                BEGIN
                CREATE TABLE OperTemplate
                (Id INT IDENTITY(1,1) NOT NULL,
                ImplementationType NVARCHAR(255) NOT NULL,
                Name NVARCHAR(255) NOT NULL,
                ConfigTemplate NVARCHAR(MAX) NOT NULL,
                CONSTRAINT [PK__OperTemplate__Id] PRIMARY KEY CLUSTERED 
                ([Id] ASC)
                )
                END;");

            connection.Execute(@"
                IF OBJECT_ID('RecepientGroup') IS NULL
                BEGIN
                CREATE TABLE RecepientGroup
                ([Id] INT IDENTITY(1,1) NOT NULL,
	            [Name] NVARCHAR(127) NOT NULL,
	            [Addresses] NVARCHAR(4000) NOT NULL,
	            [AddressesBcc] NVARCHAR(4000) NULL,
                CONSTRAINT [PK__RecepientGroup__Id] PRIMARY KEY CLUSTERED 
                ([Id] ASC)
                )
                END");

            connection.Execute(@"
                IF OBJECT_ID('TelegramChannel') IS NULL
                BEGIN
                CREATE TABLE TelegramChannel
                (Id BIGINT IDENTITY(1,1),
                Name NVARCHAR(127) NOT NULL,
                Description NVARCHAR(255) NULL,
                ChatId BIGINT NOT NULL,
                Type TINYINT NOT NULL,
                CONSTRAINT [PK__TelegramChannel__Id] PRIMARY KEY CLUSTERED 
                ([Id] ASC)
                );
                END");

            connection.Execute(@"
                IF OBJECT_ID('Schedule') IS NULL
                BEGIN
                CREATE TABLE Schedule
                (Id INT IDENTITY(1,1),
                Name NVARCHAR(127) NOT NULL,
                Schedule NVARCHAR(255) NOT NULL,
                CONSTRAINT [PK__Schedule__Id] PRIMARY KEY CLUSTERED 
                ([Id] ASC)
                );
                END");

            connection.Execute(@"
                IF OBJECT_ID('Task') IS NULL
                BEGIN
                CREATE TABLE Task
                (Id BIGINT IDENTITY(1,1),
                Name NVARCHAR(127) NOT NULL,
                ScheduleId INT NULL,
                Parameters NVARCHAR(1023) NULL,
                DependsOn NVARCHAR(1023) NULL,
                UpdateDateTime datetime NOT NULL
                CONSTRAINT [PK__Task__Id] PRIMARY KEY CLUSTERED 
                ([Id] ASC),
                CONSTRAINT FK_Task_Schedule FOREIGN KEY(ScheduleId) 
                REFERENCES Schedule(Id)
                )
                END");

            connection.Execute(@"
                IF OBJECT_ID('Operation') IS NULL
                BEGIN
                CREATE TABLE Operation
                (Id BIGINT IDENTITY(1,1),
                TaskId BIGINT NOT NULL,
                Number TINYINT NOT NULL,
                Name NVARCHAR(255) NOT NULL,
                ImplementationType NVARCHAR(255) NOT NULL,
                IsDefault BIT NOT NULL DEFAULT 0,
                Config NVARCHAR(MAX) NOT NULL,
                IsDeleted BIT NOT NULL DEFAULT 0,
                CONSTRAINT [PK__Operation__Id] PRIMARY KEY CLUSTERED 
                ([Id] ASC),
                CONSTRAINT FK_Operation_Task FOREIGN KEY(TaskId) 
                REFERENCES Task(Id))
                CREATE NONCLUSTERED INDEX [idx_Operation_TaskId] ON [dbo].[Operation]
                ([TaskId] ASC)
                END");

            connection.Execute(@"
                IF OBJECT_ID('TaskInstance') IS NULL
                BEGIN
                CREATE TABLE TaskInstance
                (Id BIGINT IDENTITY(1,1),
                TaskID BIGINT NOT NULL,
                StartTime DATETIME NOT NULL,
                Duration INT NOT NULL,
                State INT NOT NULL,
                CONSTRAINT [PK__TaskInstance__Id] PRIMARY KEY CLUSTERED 
                ([Id] ASC),
                CONSTRAINT FK_TaskInstance_Task FOREIGN KEY(TaskID)
                REFERENCES Task(Id))
                CREATE NONCLUSTERED INDEX [idx_TaskInstance_TaskId] ON [dbo].[TaskInstance]
                ([TaskID] ASC)
                END");

            connection.Execute(@"
                IF object_id('OperInstance') IS NULL
                BEGIN
                CREATE TABLE OperInstance(
                Id BIGINT IDENTITY(1,1),
	            TaskInstanceId BIGINT NOT NULL,
                OperationId BIGINT NOT NULL,
                StartTime DATETIME NOT NULL,
                Duration INT NOT NULL,
                State INT NOT NULL,
	            DataSet VARBINARY(MAX) NULL,
                ErrorMessage NVARCHAR(1023) NULL,
                CONSTRAINT [PK__OperInstance__Id] PRIMARY KEY CLUSTERED 
                ([Id] ASC),
                CONSTRAINT FK_OperInstance_Operation FOREIGN KEY(OperationId) 
                REFERENCES Operation(Id),
                CONSTRAINT FK_OperInstance_TaskInstance FOREIGN KEY(TaskInstanceId)
                REFERENCES TaskInstance(Id)
                )
                CREATE NONCLUSTERED INDEX [idx_OperInstance_OperationId] ON [dbo].[OperInstance]
                ([OperationId] ASC)
                CREATE NONCLUSTERED INDEX [idx_OperInstance_TaskInstanceId] ON [dbo].[OperInstance]
                ([TaskInstanceId] ASC)
                END");
        } //database structure creating
    }
}