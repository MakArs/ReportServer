using System;
using System.Data.SqlClient;
using System.Threading;
using System.Threading.Tasks;
using Dapper;
using Moq;
using NUnit.Framework;
using ReportService.Interfaces.Core;
using ReportService.Operations.DataExporters.Configurations;
using ReportService.Operations.DataExporters.Dependencies;
using ReportService.ReportTask;
using ReportService.Tests.App.API.Tests.Integration;
using Shouldly;

namespace ReportService.Tests.Operations.DataExporters.Dependencies
{
    [NonParallelizable]
    [TestFixture]
    public class B2BDBStructureCheckerTests
    {
        [TearDown]
        public async Task TearDown()
        {
            await using var connection = new SqlConnection(SqlTestDBHelper.TestDbConnStr);

            await SqlTestDBHelper.DropAllConstraintsAndTables(connection);
        }

        [Test]
        public void ShouldThrowException_GivenNullExporterConfig_Provided()
        {
            //Arrange
            var expectedExceptionPart = "Value cannot be null.";
            var structureChecker = new B2BDBStructureChecker();

            //Act, Assert
            Should.Throw<ArgumentNullException>(() => structureChecker.Initialize(null)).Message.ShouldContain(expectedExceptionPart);
        }

        [Test]
        public async Task ShouldThrowException_GivenNullTaskContext_Provided()
        {
            //Arrange
            var sut = new SUT
            {
                TaskContext = null
            };

            //Act, Assert
            await using var connection = new SqlConnection(SqlTestDBHelper.TestDbConnStr);

            Should.Throw<ArgumentNullException>(() => sut.StructureChecker.CheckIfDbStructureExists(connection, sut.TaskContext));
        }

        [Test]
        public async Task ShouldReturnFalse_GivenExportTableMissing()
        {
            //Arrange
            var sut = new SUT();

            //Act
            await using var connection = new SqlConnection(SqlTestDBHelper.TestDbConnStr);

            await CreateExportInstanceTable(connection, sut.ExportInstanceTableName);

            sut.StructureChecker.Initialize(sut.B2BExporterConfig);

            var exists = await sut.StructureChecker.CheckIfDbStructureExists(connection, sut.TaskContext);

            //Assert
            exists.ShouldBeFalse();
        }

        [Test]
        public async Task ShouldReturnFalse_GivenExportInstanceTableMissing()
        {
            //Arrange
            var sut = new SUT();

            //Act
            await using var connection = new SqlConnection(SqlTestDBHelper.TestDbConnStr);

            await CreateExportTable(connection, sut.ExportTableName);

            sut.StructureChecker.Initialize(sut.B2BExporterConfig);

            var exists = await sut.StructureChecker.CheckIfDbStructureExists(connection, sut.TaskContext);

            //Assert
            exists.ShouldBeFalse();
        }

        [Test]
        public async Task ShouldReturnFalse_GivenEntryWithTaskId_Missing()
        {
            //Arrange
            var sut = new SUT();

            //Act
            await using var connection = new SqlConnection(SqlTestDBHelper.TestDbConnStr);

            await CreateExportTable(connection, sut.ExportTableName);
            await CreateExportInstanceTable(connection, sut.ExportInstanceTableName);

            sut.StructureChecker.Initialize(sut.B2BExporterConfig);

            var exists = await sut.StructureChecker.CheckIfDbStructureExists(connection, sut.TaskContext);

            //Assert
            exists.ShouldBeFalse();
        }

        [Test]
        public async Task ShouldReturnTrue_GivenProperDBStructure()
        {
            //Arrange
            var sut = new SUT();

            //Act
            await using var connection = new SqlConnection(SqlTestDBHelper.TestDbConnStr);

            await CreateExportTable(connection, sut.ExportTableName);
            await CreateExportInstanceTable(connection, sut.ExportInstanceTableName);
            await CreateEntryInExportTable(connection, sut.ExportTableName, sut.TaskContext.TaskId);

            sut.StructureChecker.Initialize(sut.B2BExporterConfig);

            var exists = await sut.StructureChecker.CheckIfDbStructureExists(connection, sut.TaskContext);

            //Assert
            exists.ShouldBeTrue();
        }

        private async Task CreateExportTable(SqlConnection connection, string exportTableName)
        {

            await connection.ExecuteAsync($@"IF OBJECT_ID('{exportTableName}') IS NULL
                BEGIN
                CREATE TABLE {exportTableName}
                (Id BIGINT  NOT NULL)
                END;"); 
        }

        private async Task CreateExportInstanceTable(SqlConnection connection, string exportInstanceTableName)
        {
            await connection.ExecuteAsync($@"IF OBJECT_ID('{exportInstanceTableName}') IS NULL
                BEGIN
                CREATE TABLE {exportInstanceTableName}
                (Id BIGINT  IDENTITY(1,1),
                ReportID BIGINT  NOT NULL,
                Created DATETIME NOT NULL,
                DataPackage VARBINARY(MAX) NULL
                )
                END;"); 
        }

        private async Task CreateEntryInExportTable(SqlConnection connection, string exportTableName, long taskId)
        {
            await connection.ExecuteAsync($@"INSERT INTO {exportTableName} (Id) 
                VALUES({taskId})");
        }

        private class SUT
        {
            public string ExportTableName => "TestExportTable";
            public string ExportInstanceTableName => "TestExportInstanceTable";
            public B2BDBStructureChecker StructureChecker { get; } = new B2BDBStructureChecker();
            public B2BExporterConfig B2BExporterConfig { get; }
            public ReportTaskRunContext TaskContext { get; set; }

            public SUT()
            {
                B2BExporterConfig = new B2BExporterConfig
                {
                    ExportTableName = ExportTableName,
                    ExportInstanceTableName = ExportInstanceTableName
                };

                TaskContext = new ReportTaskRunContext(new Mock<IArchiver>().Object)
                {
                    TaskId = 0,
                    CancelSource = new CancellationTokenSource()
                };
            }
        }
    }
}
