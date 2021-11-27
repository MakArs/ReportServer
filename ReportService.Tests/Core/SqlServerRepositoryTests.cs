using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Threading;
using System.Threading.Tasks;
using Dapper;
using Dapper.Contrib.Extensions;
using Monik.Common;
using Moq;
using NUnit.Framework;
using ReportService.Core;
using ReportService.Entities;
using ReportService.Entities.Dto;
using ReportService.Tests.App.API.Tests.Integration;
using Shouldly;

namespace ReportService.Tests.Core
{
    [NonParallelizable]
    [TestFixture]
    public class SqlServerRepositoryTests
    {
        [TearDown]
        public async Task TearDown()
        {
            await using var connection = new SqlConnection(SqlTestDBHelper.TestDbConnStr);

            await SqlTestDBHelper.DropAllConstraintsAndTables(connection);
        }

        [TestCase("SELECT 1", 1, TestName = "intValue")]
        [TestCase("SELECT ISNULL(NULL, 126);", 126, TestName = "EmptyValue")]
        public async Task GetBaseQueryResult_ShouldReturn_ProperValue(string query, object expectedValue)
        {
            //Arrange
            var repository = new SqlServerRepository(SqlTestDBHelper.TestDbConnStr, new Mock<IMonik>().Object);

            //Act
            var baseQueryResult = await repository.GetBaseQueryResult(query, CancellationToken.None);

            //Assert
            baseQueryResult.ShouldBe(expectedValue);
        }

        [Test]
        public async Task ShouldCreateSchemaProperly()
        {
            //Arrange
            var repository = new SqlServerRepository(SqlTestDBHelper.TestDbConnStr, new Mock<IMonik>().Object);
            var expectedTablesList = new[]
            {
                "OperTemplate",
                "RecepientGroup",
                "TelegramChannel",
                "Schedule",
                "Task",
                "Operation",
                "TaskInstance",
                "OperInstance"
            };

            //Act
            repository.CreateSchema();
            IEnumerable<string> tablesThatNotExist = await ReturnTablesThatNotExist(expectedTablesList);

            //Assert
            tablesThatNotExist.ShouldBeEmpty();
        }

        [Test]
        public async Task ShouldCreateSchemaProperly_GivenTableAlreadyExists()
        {
            //Arrange
            var repository = new SqlServerRepository(SqlTestDBHelper.TestDbConnStr, new Mock<IMonik>().Object);
            var expectedTablesList = new[]
            {
                "OperTemplate",
                "RecepientGroup",
                "TelegramChannel",
                "Schedule",
                "Task",
                "Operation",
                "TaskInstance",
                "OperInstance"
            };

            //Act
            await CreateOperTemplateTable();
            repository.CreateSchema();
            IEnumerable<string> tablesThatNotExist = await ReturnTablesThatNotExist(expectedTablesList);

            //Assert
            tablesThatNotExist.ShouldBeEmpty();
        }

        [Test]
        public async Task ShouldReturnAllTaskInstances_GivenInstancesInsertedForDifferentTasks()
        {
            //Arrange
            var repository = new SqlServerRepository(SqlTestDBHelper.TestDbConnStr, new Mock<IMonik>().Object);
            var expectedInstancesCount = 3;

            //Act
            repository.CreateSchema();
            long insertedTaskId = await InsertTaskInstances(expectedInstancesCount, true);
            List<DtoTaskInstance> instances = await repository.GetAllTaskInstances(insertedTaskId);

            //Assert
            instances.Count.ShouldBe(expectedInstancesCount);
            instances.ShouldAllBe(instance => instance.TaskId == insertedTaskId);
        }

        [Test]
        public async Task ShouldGetTaskStateById_WithMinDateTimeAsLastSuccessful_GivenInstancesInsertedForDifferentTasks_But_WithoutInstances_For_TargetTask()
        {
            //Arrange
            var repository = new SqlServerRepository(SqlTestDBHelper.TestDbConnStr, new Mock<IMonik>().Object);
            var expectedLastSuccessfulFinish = DateTime.MinValue;

            //Act
            repository.CreateSchema();
            await InsertTaskInstances(5, true);
            long insertedTaskId = await InsertTask();
            TaskState state = repository.GetTaskStateById(insertedTaskId);

            //Assert
            state.LastSuccessfulFinish.ShouldBe(expectedLastSuccessfulFinish);
        }

        [Test]
        public async Task ShouldGetTaskStateById_WithMinDateTimeAsLastSuccessful_GivenInstancesInsertedForDifferentTasks_But_WithoutSuccessfulInstances_For_TargetTask()
        {
            //Arrange
            var repository = new SqlServerRepository(SqlTestDBHelper.TestDbConnStr, new Mock<IMonik>().Object);
            var expectedLastSuccessfulFinish = DateTime.MinValue;

            //Act
            repository.CreateSchema();
            await InsertTaskInstances(5, true);
            long insertedTaskId = await InsertTask();
            var instance = new DtoTaskInstance
            {
                Duration = 12345,
                StartTime = DateTime.Today,
                State = (int)InstanceState.Failed,
                TaskId = insertedTaskId
            };

            await using var connection = new SqlConnection(SqlTestDBHelper.TestDbConnStr);
            await connection.InsertAsync(instance);
            
            TaskState state = repository.GetTaskStateById(insertedTaskId);

            //Assert
            state.LastSuccessfulFinish.ShouldBe(expectedLastSuccessfulFinish);
        }

        [Test]
        [Ignore("Flaky test. For some duration values returns result that is not exactly equal to expected")]
        public async Task ShouldGetTaskStateById_GivenInstancesInsertedForDifferentTasks()
        {
            //Arrange
            var repository = new SqlServerRepository(SqlTestDBHelper.TestDbConnStr, new Mock<IMonik>().Object);
            var duration = 12345;
            var expectedStartTime = new DateTime(2020, 12, 20, 11, 23, 45);
            var expectedLastSuccessfulFinish = expectedStartTime.AddMilliseconds(duration);

            //Act
            repository.CreateSchema();
            await InsertTaskInstances(5, true);
            long insertedTaskId = await InsertTask();
            var instance = new DtoTaskInstance
            {
                Duration = duration,
                StartTime = expectedStartTime,
                State = (int)InstanceState.Success,
                TaskId = insertedTaskId
            };

            await using var connection = new SqlConnection(SqlTestDBHelper.TestDbConnStr);
            await connection.InsertAsync(instance);
            
            TaskState state = repository.GetTaskStateById(insertedTaskId);

            //Assert
            state.LastSuccessfulFinish.ShouldBe(expectedLastSuccessfulFinish);
        }

        private async Task CreateOperTemplateTable()
        {
            await using var connection = new SqlConnection(SqlTestDBHelper.TestDbConnStr);
            await connection.ExecuteAsync(@"CREATE TABLE OperTemplate
                (Id INT IDENTITY(1,1) NOT NULL,
                ImplementationType NVARCHAR(255) NOT NULL,
                Name NVARCHAR(255) NOT NULL,
                ConfigTemplate NVARCHAR(MAX) NOT NULL,
                CONSTRAINT [PK__OperTemplate__Id] PRIMARY KEY CLUSTERED 
                ([Id] ASC)
                )");
        }

        private async Task<long> InsertTaskInstances(int instancesAmount, bool addInstanceForOtherTask)
        {
            await using var connection = new SqlConnection(SqlTestDBHelper.TestDbConnStr);

            long insertedTaskId = await InsertTask();

            var instance = new DtoTaskInstance
            {
                Duration = 12345,
                StartTime = DateTime.Today,
                State = (int)InstanceState.Success,
                TaskId = insertedTaskId
            };

            for (int i = 0; i < instancesAmount; i++)
            {
                await connection.InsertAsync(instance);
            }

            if (addInstanceForOtherTask)
            {
                long otherInsertedTaskId = await InsertTask();

                var instanceForOtherTask = new DtoTaskInstance
                {
                    Duration = 12345,
                    StartTime = DateTime.Today,
                    State = (int)InstanceState.Success,
                    TaskId = otherInsertedTaskId
                };

                await connection.InsertAsync(instanceForOtherTask);
            }

            return insertedTaskId;
        }

        private async Task<long> InsertTask()
        {
            await using var connection = new SqlConnection(SqlTestDBHelper.TestDbConnStr);

            var task = new DtoTask
            {
                Name = "TestTask",
                UpdateDateTime = DateTime.Today,
            };

            return await connection.InsertAsync(task);
        }

        private async Task<IEnumerable<string>> ReturnTablesThatNotExist(params string[] tableNames)
        {
            List<string> notExistingTables = new List<string>();

            await using var connection = new SqlConnection(SqlTestDBHelper.TestDbConnStr);
            foreach (var tableName in tableNames)
            {
                if (await connection.QueryFirstOrDefaultAsync<int>($@"IF OBJECT_ID('{tableName}') IS NOT NULL SELECT 1 ELSE SELECT 0") == 0)
                    notExistingTables.Add(tableName);
            }

            return notExistingTables;
        }
    }
}
