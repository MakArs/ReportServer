﻿using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Ardalis.GuardClauses;
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
        public int DtoPrefixLength => 3;

        private readonly IMonik mMonik;
        private readonly string mConnectionString;

        public SqlServerRepository(string connStr, IMonik monik)
        {
            Guard.Against.Null(monik, nameof(monik));
            Guard.Against.NullOrEmpty(connStr, nameof(connStr));

            mConnectionString = connStr;
            mMonik = monik;
        }

        public async Task<object> GetBaseQueryResult(string query, CancellationToken token)
        {
            await using var connection = new SqlConnection(mConnectionString);

            dynamic result = await connection.QueryFirstAsync<dynamic>(new CommandDefinition(query, commandTimeout: 30, cancellationToken: token));

            object value = (result as IDictionary<string, object>)?.First().Value;
            return value;
        }

        public async Task<List<DtoTaskInstance>> GetAllTaskInstances(long taskId)
        {
            await using var connection = new SqlConnection(mConnectionString);

            try
            {
                return (await connection.QueryAsync<DtoTaskInstance>($"select * from TaskInstance with(nolock) where TaskId={taskId}"))
                    .ToList();
            }

            catch (Exception e)
            {
                SendAppWarning($"Error occurred while getting task instances: {e.Message}");
                throw;
            }
        }

        public TaskState GetTaskStateById(long taskId) //todo: check why dateadd in SQL returns a bit different result for some values of argument(ms)
        {
            using var connection = new SqlConnection(mConnectionString);

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
                SendAppWarning($"Error occurred while getting task last time: {e.Message}");
                throw;
            }
        }

        public List<DtoOperInstance> GetTaskOperInstances(long taskInstanceId)
        {
            using var connection = new SqlConnection(mConnectionString);

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
                SendAppWarning($"Error occurred while getting operation instances: {e.Message}");
                throw;
            }
        }

        public List<DtoOperInstance> GetFullTaskOperInstances(long taskInstanceId)
        {
            using var connection = new SqlConnection(mConnectionString);

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
            using var connection = new SqlConnection(mConnectionString);

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
                SendAppWarning($"Error occurred while getting operation instance data: {e.Message}");
                throw;
            }
        }

        public List<T> GetListEntitiesByDtoType<T>() where T : class, IDtoEntity
        {
            using var connection = new SqlConnection(mConnectionString);

            string tableName = typeof(T).Name.Remove(0, DtoPrefixLength);

            try
            {
                return connection.GetAll<T>().ToList();
            }

            catch (Exception e)
            {
                SendAppWarning($"Error occurred while getting {tableName} list: {e.Message}");
                return null;
            }
        }

        public long CreateEntity<T>(T entity) where T : class, IDtoEntity
        {
            using var connection = new SqlConnection(mConnectionString);

            string tableName = typeof(T).Name.Remove(0, DtoPrefixLength);

            try
            {
                return connection.Insert(entity, commandTimeout: 60);
            }

            catch (Exception e)
            {
                SendAppWarning($"Error occurred while creating new {tableName} record: {e.Message}");
                return default;
            }
        }

        public long CreateTask(DtoTask task, params DtoOperation[] bindedOpers)
        {
            using var connection = new SqlConnection(mConnectionString);
            connection.Open();

            long newTaskId;

            using (var transaction = connection.BeginTransaction())
            {
                try
                {
                    newTaskId = connection.Insert(task, commandTimeout: 60, transaction: transaction);

                    if (bindedOpers != null)
                        foreach (var oper in bindedOpers)
                        {
                            oper.TaskId = newTaskId;
                        }

                    connection.Insert(bindedOpers, commandTimeout: 60, transaction: transaction);
                    transaction.Commit();
                }

                catch (Exception e)
                {
                    transaction.Rollback();

                    SendAppWarning($"Error occurred while creating new Task record: {e.Message}");
                    throw;
                }
            }

            return newTaskId;
        }

        public void UpdateEntity<T>(T entity) where T : class, IDtoEntity
        {
            using var connection = new SqlConnection(mConnectionString);

            string tableName = typeof(T).Name.Remove(0, DtoPrefixLength);

            try
            {
                connection.Update(entity, commandTimeout: 60);
            }

            catch (Exception e)
            {
                SendAppWarning($"Error occurred while updating {tableName} record: {e.Message}");
            }
        }

        public void UpdateTask(DtoTask task, params DtoOperation[] bindedOpers)
        {
            using var connection = new SqlConnection(mConnectionString);
            connection.Open();

            long[] currentOperIds = connection.Query<long>
            ($@"select id from operation with(nolock) where taskid={task.Id}
                and isDeleted=0",
                commandTimeout: 60).ToArray();

            using var transaction = connection.BeginTransaction();
            try
            {
                long[] newOperIds = bindedOpers.Select(oper => oper.Id).ToArray();

                long[] operIdsToDelete = currentOperIds.Except(newOperIds).ToArray();
                IEnumerable<DtoOperation> opersToUpdate = bindedOpers.Where(oper => newOperIds.Intersect(currentOperIds).Contains(oper.Id));
                IEnumerable<DtoOperation> opersToWrite = bindedOpers.Where(oper => newOperIds.Except(currentOperIds).Contains(oper.Id));

                connection.Update(task, commandTimeout: 60, transaction: transaction);

                if (operIdsToDelete.Any())
                    connection.Execute($@"Update Operation set isDeleted=1 where TaskId={task.Id} and
                            id in ({string.Join(",", operIdsToDelete)})",
                        commandTimeout: 60, transaction: transaction);

                connection.Update(opersToUpdate, commandTimeout: 60, transaction: transaction);
                connection.Insert(opersToWrite, commandTimeout: 60, transaction: transaction);
                transaction.Commit();
            }

            catch (Exception e)
            {
                transaction.Rollback();

                SendAppWarning($"Error occurred while updating Task record: {e.Message}");
                throw;
            }
        }

        public void DeleteEntity<T, TKey>(TKey id) where T : IDtoEntity
        {
            using var connection = new SqlConnection(mConnectionString);

            var type = typeof(T);
            string tableName = type.Name.Remove(0, DtoPrefixLength);

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
                            transaction.Rollback();

                            SendAppWarning($"Error occurred while deleting Task instance record: {e.Message}");
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

                            SendAppWarning($"Error occurred while deleting Task record: {e.Message}");
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
                        SendAppWarning($"Error occurred while deleting {tableName} record: {e.Message}");
                        throw;
                    }

                    break;
            }
        }

        public List<long> UpdateOperInstancesAndGetIds()
        {
            using var connection = new SqlConnection(mConnectionString);

            List<long> ids = connection.Query<long>(@"UPDATE OperInstance
            SET state=3,ErrorMessage='Unknown error.The service was probably stopped during the task execution.'
            OUTPUT INSERTED.id
            WHERE state=1").ToList();

            return ids;
        }

        public List<long> UpdateTaskInstancesAndGetIds()
        {
            using var connection = new SqlConnection(mConnectionString);

            List<long> ids = connection.Query<long>(@"UPDATE TaskInstance
            SET state=3
            OUTPUT INSERTED.id
            WHERE state=1").ToList();

            return ids;
        }

        private void SendAppWarning(string msg)
        {
            mMonik.ApplicationWarning(msg);
            Console.WriteLine(msg);
        }

        public long CreateTaskRequestInfo(TaskRequestInfo taskRequestInfo)
        {
            using var connection = new SqlConnection(mConnectionString);

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

                catch (Exception)
                {
                    transaction.Rollback();
                    throw;
                }
            }

            return newTaskRequestInfoId;
        }

        public List<TaskRequestInfo> GetListTaskRequestInfoByIds(long[] taskRequestIds)
        {
            using var connection = new SqlConnection(mConnectionString);

            try
            {
                return connection.Query<TaskRequestInfo>
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
                where tri.RequestId in ({string.Join(",", taskRequestIds)})",
                    commandTimeout: 60).ToList();
            }

            catch (Exception e)
            {
                SendAppWarning("Error occured while getting task request info data: " +
                               $"{e.Message}");
                throw;
            }
        }

        public TaskRequestInfo GetTaskRequestInfoById(long taskRequestId)
        {
            using var connection = new SqlConnection(mConnectionString);

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

        public List<TaskRequestInfo> GetTaskRequestInfoByFilter(RequestStatusFilter requestStatusFilter )
        {
            using var connection = new SqlConnection(mConnectionString);

            var builder = new SqlBuilder();
            var selector = builder.AddTemplate
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
                /**where**/"
                );

            if (requestStatusFilter.TaskIds != null && requestStatusFilter.TaskIds.Any())
            {
                builder.Where(
                        "tri.TaskId in @taskIds",
                        new { taskIds = requestStatusFilter.TaskIds }
                    );
            }

            if (requestStatusFilter.TaskRequestInfoIds != null &&  requestStatusFilter.TaskRequestInfoIds.Any())
            {
                builder.Where(
                        "tri.RequestId in @taskRequestInfoIds",
                        new { taskRequestInfoIds = requestStatusFilter.TaskRequestInfoIds }
                    );
            }

            if (requestStatusFilter.TimePeriod != null)
            {
                builder.Where(
                       "tri.CreateTime between @dateFrom and @dateTo",
                       new { dateFrom = requestStatusFilter.TimePeriod.DateFrom, dateTo = requestStatusFilter.TimePeriod.DateTo }
                    );
            }

            if (requestStatusFilter.Status != null)
            {
                builder.Where(
                        "tri.Status = @status",
                        new { status = requestStatusFilter.Status }
                    );
            }

            return connection.Query<TaskRequestInfo>(selector.RawSql, selector.Parameters).ToList();
        }

        public List<TaskRequestInfo> GetTaskRequestInfoByTimePeriod(DateTime timeFrom, DateTime timeTo)
        {
            using var connection = new SqlConnection(mConnectionString);

            try
            {
                return connection.Query<TaskRequestInfo>
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
                where tri.CreateTime between '{timeFrom.ToString("yyyy-MM-dd HH:mm:ss")}' and '{timeTo.ToString("yyyy-MM-dd HH:mm:ss")}'",
                    commandTimeout: 60).ToList();
            }
            catch (Exception e)
            {
                SendAppWarning("Error occured while getting task request info data: " +
                               $"{e.Message}");
                throw;
            }
        }

        public List<TaskRequestInfo> GetTaskRequestInfoByTaskIds(long[] taskIds)
        {
            using var connection = new SqlConnection(mConnectionString);

            try
            {
                return connection.Query<TaskRequestInfo>
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
                where tri.TaskId in ({string.Join(",", taskIds)})",
                    commandTimeout: 60).ToList();
            }

            catch (Exception e)
            {
                SendAppWarning("Error occured while getting task request info data: " +
                               $"{e.Message}");
                throw;
            }
        }

        public List<DtoTask> GetTasksFromDb(long[] taskIds)
        {
            using var connection = new SqlConnection(mConnectionString);

            try
            {
                return connection.Query<DtoTask>
                (
                    $@"
                        select
                            t.Id,
                            t.Name,
                            t.ScheduleId,
                            t.Parameters,
                            t.DependsOn,
                            t.UpdateDateTime,
                            t.ParameterInfos
                        from Task t with(nolock)
                        where t.Id in ({string.Join(",", taskIds)})",
                    commandTimeout: 60).ToList();
            }

            catch (Exception e)
            {
                SendAppWarning("Error occured while getting tasks info data: " +
                               $"{e.Message}");
                throw;
            }
        }

        public DtoTask GetTaskFromDb(long taskId)
        {
            using var connection = new SqlConnection(mConnectionString);

            try
            {
                return connection.QueryFirst<DtoTask>
                (
                    $@"
                        select
                            t.Id,
                            t.Name,
                            t.ScheduleId,
                            t.Parameters,
                            t.DependsOn,
                            t.UpdateDateTime,
                            t.ParameterInfos
                        from Task t with(nolock)
                        where t.Id = {taskId}",
                    commandTimeout: 60);
            }

            catch (Exception e)
            {
                SendAppWarning("Error occured while getting tasks info data: " +
                               $"{e.Message}");
                throw;
            }
        }

        public void CreateSchema()
        {
            using var connection = new SqlConnection(mConnectionString);

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
                ParameterInfos NVARCHAR(1023) NULL,
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
