using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Dapper;
using Dapper.Contrib.Extensions;
using Monik.Common;
using Npgsql;
using ReportService.Entities;
using ReportService.Entities.Dto;
using ReportService.Interfaces.Core;

namespace ReportService.Core
{
    //dapper needs full 
    public class PostgreSqlRepository : IRepository
    {
        public int DtoPrefixLength => 3;

        private readonly IMonik mMonik;
        private readonly string mConnectionString;

        public PostgreSqlRepository(string connStr, IMonik monik)
        {
            mConnectionString = connStr;
            mMonik = monik;
        }

        public async Task<object> GetBaseQueryResult(string query, CancellationToken token)
        {
            await using var connection = new NpgsqlConnection(mConnectionString);

            dynamic result = await connection.QueryFirstAsync<dynamic>(new CommandDefinition(query, commandTimeout: 30, cancellationToken: token));

            object value = (result as IDictionary<string, object>)?.First().Value;
            return value;
        }

        public async Task<List<DtoTaskInstance>> GetAllTaskInstances(long taskId)
        {
            await using var connection = new NpgsqlConnection(mConnectionString);

            try
            {
                //todo: create querybuilder
                return (await connection.QueryAsync<DtoTaskInstance>($@"select * from ""TaskInstance"" where ""TaskId""={taskId}")).ToList();
            }

            catch (Exception e)
            {
                SendAppWarning($"Error occurred while getting task instances: {e.Message}");
                throw;
            }
        }

        public TaskState GetTaskStateById(long taskId)
        {
            using var connection = new NpgsqlConnection(mConnectionString); 

            try
            {
                return connection.QueryFirst<TaskState>(
                    $@"SELECT max(case when ""State""=2 then ""StartTime""+ ""Duration""*INTERVAL'1 ms' 
		                        else null end) LastSuccessfulFinish,
	                                        count(case when ""State""=1 then 1 else null end) InProcessCount,
                                            case when max(ti.""StartTime"")>max(t.""UpdateDateTime"") then max(ti.""StartTime"")
											else  max(t.""UpdateDateTime"") end LastStart
                                            FROM
											""Task"" t
											left join ""TaskInstance"" ti 
											on t.""Id""=ti.""TaskId""
											where t.""Id""={taskId}",
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
            using var connection = new NpgsqlConnection(mConnectionString);

            try
            {
                return connection.Query<DtoOperInstance>
                    ($@"select ""Id"",""TaskInstanceId"",""OperationId"",
                            ""StartTime"",""Duration"",""State"",null as DataSet,
                            null as ErrorMessage
                        from ""OperInstance"" where ""TaskInstanceId""={taskInstanceId}",
                        commandTimeout: 60).ToList();
            }

            catch (Exception e)
            {
                SendAppWarning($"Error occurred while getting operation instances: {e.Message}");
                throw;
            }
        }

        public List<DtoOperInstance> GetFullTaskOperInstances(long taskInstanceId)
        {
            using var connection = new NpgsqlConnection(mConnectionString);

            try
            {
                return connection.Query<DtoOperInstance>
                    ($@"select ""Id"",""TaskInstanceId"",""OperationId"",
                            ""StartTime"",""Duration"",""State"",DataSet,
                            ErrorMessage
                        from ""OperInstance"" where ""TaskInstanceId""={taskInstanceId}",
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
            using var connection = new NpgsqlConnection(mConnectionString);

            try
            {
                return connection.QueryFirst<DtoOperInstance>
                ($@"select oi.""Id"",""TaskInstanceId"",""OperationId"",
                    ""StartTime"",""Duration"",""State"",""DataSet"",
                    ""ErrorMessage"",""Name"" as OperName
                from ""OperInstance"" oi
                join ""Operation"" op
                    on oi.""OperationId"" = op.""Id""
                where oi.""Id"" ={operInstanceId}",
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
            using var connection = new NpgsqlConnection(mConnectionString);

            string tableName = typeof(T).Name.Remove(0, DtoPrefixLength);

            try
            {
                return connection.GetAll<T>().ToList();
            }

            catch (Exception e)
            {
                SendAppWarning("Error occurred while getting " +
                               $"{tableName} list: {e.Message}");
                return null;
            }
        }

        public long CreateEntity<T>(T entity) where T : class, IDtoEntity
        {
            using var connection = new NpgsqlConnection(mConnectionString);

            string tableName = typeof(T).Name.Remove(0, DtoPrefixLength);

            var insertString = tableName switch
            {
                "OperInstance" => cOperInstanceInsertString,
                "OperTemplate" => cOperTemplateInsertString,
                "RecepientGroup" => cRecipientGroupInsertString,
                "Schedule" => cScheduleInsertString,
                "TaskInstance" => cTaskInstanceInsertString,
                "TelegramChannel" => cTelegramChannelInsertString,
                _ => throw new Exception("Type not recognized as part of service"),
            };

            try
            {
                long newId = connection.QueryFirst<long>(insertString, entity, commandTimeout: 60);
                return newId;
            }

            catch (Exception e)
            {
                SendAppWarning($"Error occurred while creating new {tableName} record: {e.Message}");
                return default;
            }
        }

        private const string cOperInstanceInsertString = @"INSERT INTO ""OperInstance""
            (""TaskInstanceId"",""OperationId"",""StartTime"",""Duration"",""State"", ""DataSet"", ""ErrorMessage"")
            VALUES(@TaskInstanceId, @OperationId, @StartTime, @Duration, @State, @DataSet, @ErrorMessage)
            RETURNING ""Id""; ";

        private const string cOperTemplateInsertString = @"INSERT INTO ""OperTemplate""
            (""ImplementationType"",""Name"",""ConfigTemplate"")
            VALUES(@ImplementationType, @Name, @ConfigTemplate)
            RETURNING ""Id""; ";

        private const string cRecipientGroupInsertString = @"INSERT INTO ""RecepientGroup""
            (""Name"",""Addresses"",""AddressesBcc"")
            VALUES(@Name, @Addresses, @AddressesBcc)
            RETURNING ""Id""; ";

        private const string cScheduleInsertString = @"INSERT INTO ""Schedule""
            (""Name"",""Schedule"")
            VALUES(@Name, @Schedule)
            RETURNING ""Id""; ";

        private const string cTaskInstanceInsertString = @"INSERT INTO ""TaskInstance""
            (""TaskId"",""StartTime"",""Duration"",""State"")
            VALUES(@TaskId, @StartTime, @Duration, @State)
            RETURNING ""Id""; ";

        private const string cTelegramChannelInsertString = @"INSERT INTO ""TelegramChannel""
            (""TaskId"",""StartTime"",""Duration"",""State"")
            VALUES(@TaskId, @StartTime, @Duration, @State)
            RETURNING ""Id""; ";

        public long CreateTask(DtoTask task, params DtoOperation[] bindedOpers)
        {
            long newTaskId;
            
            using var connection = new NpgsqlConnection(mConnectionString);
            connection.Open();

            using var transaction = connection.BeginTransaction();
            try
            {
                newTaskId = connection.QueryFirst<long>(@"INSERT INTO ""Task""
                        (""Name"", ""ScheduleId"", ""Parameters"", ""DependsOn"")
                        VALUES(@Name, @ScheduleId, @Parameters, @DependsOn)
                        RETURNING ""Id"";", 
                    task, commandTimeout: 60, transaction: transaction);

                if (bindedOpers != null)
                    foreach (var oper in bindedOpers)
                    {
                        oper.TaskId = newTaskId;
                    }

                connection.Execute(@"INSERT INTO ""Operation""(
                                    ""TaskId"",""Number"",""Name"",""ImplementationType"",""IsDefault"",""Config"",""IsDeleted"")
                                     VALUES(@TaskId, @Number, @Name, @ImplementationType, @IsDefault, @Config, @IsDeleted); ",
                    bindedOpers, commandTimeout: 60, transaction: transaction);

                transaction.Commit();
            }

            catch (Exception e)
            {
                transaction.Rollback();

                SendAppWarning($"Error occurred while creating new Task record: {e.Message}");
                throw;
            }

            return newTaskId;
        }

        public void UpdateEntity<T>(T entity) where T : class, IDtoEntity
        {
            using var connection = new NpgsqlConnection(mConnectionString);
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
            using var connection = new NpgsqlConnection(mConnectionString);
            connection.Open();

            long[] currentOperIds = connection.Query<long>
            ($@"select ""Id"" from ""Operation"" where ""TaskId""={task.Id}
                and ""IsDeleted""=false",
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
                    connection.Execute($@"Update ""Operation"" set ""IsDeleted""=True where ""TaskId""={task.Id} and
                            ""Id"" in ({string.Join(",", operIdsToDelete)})",
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
            using var connection = new NpgsqlConnection(mConnectionString);

            var type = typeof(T);
            string tableName = type.Name.Remove(0, DtoPrefixLength);

            connection.Open();

            switch (true)
            {
                //todo: extract logic?
                case { } when type == typeof(DtoTaskInstance):
                    using (var transaction = connection.BeginTransaction())
                    {
                        try
                        {
                            connection.Execute($@"delete from ""OperInstance"" where ""TaskInstanceId""={id}",
                                commandTimeout: 60, transaction: transaction);

                            connection.Execute($@"delete from ""TaskInstance"" where ""Id""={id}",
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
                            connection.Execute($@"delete from ""OperInstance"" where ""TaskInstanceId"" in
                                            (select ""Id"" from ""TaskInstance"" where ""TaskId""={id})",
                                commandTimeout: 60, transaction: transaction);

                            connection.Execute($@"delete from ""TaskInstance"" where ""TaskId""={id}",
                                commandTimeout: 60, transaction: transaction);

                            connection.Execute($@"delete from ""Operation"" where ""TaskId""={id}",
                                commandTimeout: 60, transaction: transaction);

                            connection.Execute($@"delete from ""Task"" where ""Id""={id}",
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
                        connection.Execute($@"delete from ""{tableName}"" where ""Id""={id}",
                            commandTimeout: 60);
                    }

                    catch (Exception e)
                    {
                        SendAppWarning($"Error occurred while deleting {tableName} record: {e.Message}");
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
            using var connection = new NpgsqlConnection(mConnectionString);

            List<long> ids = connection.Query<long>(@"UPDATE ""OperInstance""
            SET ""State""=3,""ErrorMessage""='Unknown error.The service was probably stopped during the task execution.'            
            WHERE ""State""=1
			RETURNING ""Id""").ToList();

            return ids;
        }

        public List<long> UpdateTaskInstancesAndGetIds()
        {
            using var connection = new NpgsqlConnection(mConnectionString);

            List<long> ids = connection.Query<long>(@"UPDATE ""TaskInstance""
            SET ""State""=3
            WHERE ""State""=1
            RETURNING ""Id""").ToList();

            return ids;
        }

        private void SendAppWarning(string msg)
        {
            mMonik.ApplicationWarning(msg);
            Console.WriteLine(msg);
        }

        public long CreateTaskRequestInfo(TaskRequestInfo taskRequestInfo)
        {
            return 0;
        }

        public List<TaskRequestInfo> GetListTaskRequestInfoByIds(long[] taskRequestIds)
        { 
            return null; 
        }

        public TaskRequestInfo GetTaskRequestInfoById(long taskRequestId)
        {
            using var connection = new NpgsqlConnection(mConnectionString);

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
                from TaskRequestInfo tri
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

        public List<TaskRequestInfo> GetTaskRequestInfoByFilter(RequestStatusFilter requestStatusFilter)
        {
            return null;
        }

        public List<TaskRequestInfo> GetTaskRequestInfoByTimePeriod(DateTime timeFrom, DateTime timeTo)
        { 
            return null;
        }

        public List<TaskRequestInfo> GetTaskRequestInfoByTaskIds(long[] taskIds)
        {
            return null;
        }

        public List<DtoTask> GetTasksFromDb(long[] taskIds)
        {
            return null;
        }

        public DtoTask GetTaskFromDb(long taskId) 
        {
            return null;
        }

        public void CreateSchema()
        {
            using var connection = new NpgsqlConnection(mConnectionString);
            
            connection.Execute(@"
                CREATE TABLE IF NOT EXISTS ""OperTemplate""
                (""Id"" INT GENERATED ALWAYS AS IDENTITY (START WITH 1 INCREMENT BY 1),
                ""ImplementationType"" VARCHAR(255) NOT NULL,
                ""Name"" VARCHAR(255) NOT NULL,
                ""ConfigTemplate"" TEXT NOT NULL,
                CONSTRAINT ""PK_OperTemplate_Id"" PRIMARY KEY(""Id""));
                CLUSTER ""OperTemplate"" USING ""PK_OperTemplate_Id"";");

            connection.Execute(@"
                CREATE TABLE IF NOT EXISTS ""RecepientGroup""
                (""Id"" INT GENERATED ALWAYS AS IDENTITY (START WITH 1 INCREMENT BY 1),
	            ""Name"" VARCHAR(127) NOT NULL,
	            ""Addresses"" VARCHAR(4000) NOT NULL,
	            ""AddressesBcc"" VARCHAR(4000) NULL,
                CONSTRAINT ""PK_RecepientGroup_Id"" PRIMARY KEY(""Id""));
				CLUSTER ""RecepientGroup"" USING ""PK_RecepientGroup_Id"";");

            connection.Execute(@"
                CREATE TABLE IF NOT EXISTS ""TelegramChannel""
                (""Id"" BIGINT GENERATED ALWAYS AS IDENTITY (START WITH 1 INCREMENT BY 1),
                ""Name"" VARCHAR(127) NOT NULL,
                ""Description"" VARCHAR(255) NULL,
                ""ChatId"" BIGINT NOT NULL,
                ""Type"" SMALLINT NOT NULL,
                CONSTRAINT ""PK_TelegramChannel_Id"" PRIMARY KEY(""Id""));
                CLUSTER ""TelegramChannel"" USING ""PK_TelegramChannel_Id"";");

            connection.Execute(@"
                CREATE TABLE IF NOT EXISTS ""Schedule""
                (""Id"" INT GENERATED ALWAYS AS IDENTITY (START WITH 1 INCREMENT BY 1),
                ""Name"" VARCHAR(127) NOT NULL,
                ""Schedule"" VARCHAR(255) NOT NULL,
                CONSTRAINT ""PK_Schedule_Id"" PRIMARY KEY(""Id""));
				CLUSTER ""Schedule"" USING ""PK_Schedule_Id"";");

            connection.Execute(@"
                CREATE TABLE IF NOT EXISTS ""Task""
                (""Id"" BIGINT GENERATED ALWAYS AS IDENTITY (START WITH 1 INCREMENT BY 1),
                ""Name"" VARCHAR(127) NOT NULL,
                ""ScheduleId"" INT NULL,
                ""Parameters"" VARCHAR(1023) NULL,
                ""DependsOn"" VARCHAR(1023) NULL,
                ""UpdateDateTime"" TIMESTAMP(3) NOT NULL,
                CONSTRAINT ""PK_Task_Id"" PRIMARY KEY(""Id""),
                CONSTRAINT ""FK_Task_Schedule"" FOREIGN KEY(""ScheduleId"") 
                REFERENCES ""Schedule""(""Id"")
                );
				CLUSTER ""Task"" USING ""PK_Task_Id"";");

            connection.Execute(@"
                CREATE TABLE IF NOT EXISTS ""Operation""
                (""Id"" BIGINT GENERATED ALWAYS AS IDENTITY (START WITH 1 INCREMENT BY 1),
                ""TaskId"" BIGINT NOT NULL,
                ""Number"" SMALLINT NOT NULL,
                ""Name"" VARCHAR(255) NOT NULL,
                ""ImplementationType"" VARCHAR(255) NOT NULL,
                ""IsDefault"" BOOLEAN NOT NULL DEFAULT FALSE,
                ""Config"" TEXT NOT NULL,
                ""IsDeleted"" BOOLEAN NOT NULL DEFAULT FALSE,
                CONSTRAINT ""PK_Operation_Id"" PRIMARY KEY(""Id""),
                CONSTRAINT ""FK_Operation_Task"" FOREIGN KEY(""TaskId"") 
                REFERENCES ""Task""(""Id""));
				CLUSTER ""Operation"" USING ""PK_Operation_Id"";
                CREATE INDEX IF NOT EXISTS ""idx_Operation_TaskId"" ON ""Operation""(""TaskId"" ASC);");

            connection.Execute(@"
                CREATE TABLE IF NOT EXISTS ""TaskInstance""
                (""Id"" BIGINT GENERATED ALWAYS AS IDENTITY (START WITH 1 INCREMENT BY 1),
                ""TaskId"" BIGINT NOT NULL,
                ""StartTime"" TIMESTAMP(3) NOT NULL,
                ""Duration"" INT NOT NULL,
                ""State"" INT NOT NULL,
                CONSTRAINT ""PK_TaskInstance_Id"" PRIMARY KEY(""Id"") ,
                CONSTRAINT ""FK_TaskInstance_Task"" FOREIGN KEY(""TaskId"")
                REFERENCES ""Task""(""Id""));
				CLUSTER ""TaskInstance"" USING ""PK_TaskInstance_Id"";
                CREATE INDEX IF NOT EXISTS ""idx_TaskInstance_TaskId"" ON ""TaskInstance""(""TaskId"" ASC);");

            connection.Execute(@"
                CREATE TABLE IF NOT EXISTS ""OperInstance""
				(""Id"" BIGINT GENERATED ALWAYS AS IDENTITY (START WITH 1 INCREMENT BY 1),
	            ""TaskInstanceId"" BIGINT NOT NULL,
                ""OperationId"" BIGINT NOT NULL,
                ""StartTime"" TIMESTAMP(3) NOT NULL,
                ""Duration"" INT NOT NULL,
                ""State"" INT NOT NULL,
	            ""DataSet"" BYTEA NULL,
                ""ErrorMessage"" VARCHAR(1023) NULL,
                CONSTRAINT ""PK_OperInstance_Id"" PRIMARY KEY(""Id""),
                CONSTRAINT ""FK_OperInstance_Operation"" FOREIGN KEY(""OperationId"") 
                REFERENCES ""Operation""(""Id""),
                CONSTRAINT ""FK_OperInstance_TaskInstance"" FOREIGN KEY(""TaskInstanceId"")
                REFERENCES ""TaskInstance""(""Id""));
				CLUSTER ""OperInstance"" USING ""PK_OperInstance_Id"";
                CREATE INDEX IF NOT EXISTS ""idx_OperInstance_OperationId"" ON ""OperInstance""(""OperationId"" ASC);
                CREATE INDEX IF NOT EXISTS ""idx_OperInstance_TaskInstanceId"" ON ""OperInstance""(""TaskInstanceId"" ASC);");
        }//database structure creation
    }
}
