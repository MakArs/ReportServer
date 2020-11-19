using Autofac;
using AutoMapper;
using Dapper;
using Microsoft.Extensions.Configuration;
using Monik.Common;
using Moq;
using Newtonsoft.Json;
using ReportService.Core;
using ReportService.Entities;
using ReportService.Entities.Dto;
using ReportService.Extensions;
using ReportService.Interfaces.Core;
using ReportService.Interfaces.Operations;
using ReportService.Interfaces.Protobuf;
using ReportService.Interfaces.ReportTask;
using ReportService.Operations.DataExporters;
using ReportService.Operations.DataExporters.Configurations;
using ReportService.Operations.DataExporters.Dependencies;
using ReportService.Operations.DataExporters.ViewExecutors;
using ReportService.Operations.DataImporters;
using ReportService.Operations.DataImporters.Configurations;
using ReportService.Operations.Helpers;
using ReportService.Protobuf;
using ReportService.ReportTask;
using ReportService.Tests.App.API.Tests.Integration;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Xunit;

using IConfigurationProvider = Microsoft.Extensions.Configuration.IConfigurationProvider;

namespace ReportService.Tests
{

    public class DbExporterTests
    {

        #region fields

        private APIWebApplicationFactory _factory;
        private HttpClient _client;
        private ILifetimeScope autofac;
        private readonly ContainerBuilder builder;
        private const string ReportServerTestDbName = "ReportServerTestDb";
        private const string WorkTableName = "TestTable";
        private const string TestDbConnStr = "Server=localhost,11433;User Id=sa;Password=P@55w0rd;Timeout=5";
        #endregion


        #region ctor

        public DbExporterTests()
        {
            builder = new ContainerBuilder();
            ConfigureMapper(builder);
            //_factory = new APIWebApplicationFactory();
            //_client = _factory.CreateClient();
        }
        #endregion


        #region testing methods

        [Fact]
        public async Task TestExportPackageScriptCreator()
        {
            IContainer autofac = InitializeTestContainer();
            await ConfigureDbForTestScenario();

            var dtoTask = new DtoTask();
            dtoTask.Id = 1;
            dtoTask.Name = $"{nameof(DbImporter)} - {nameof(DbPackageDataConsumer)} Test";

            // setup a simple db import operation, which initializes an operaion package in the taskContext:
            string packageInitializingQuery1 = @"select id, ConfigTemplate 
from [dbo].[OperTemplate]";
            CreateOperation(dtoTask.Id
               , 1
               , nameof(DbImporter)
               , packageInitializingQuery1
               , "PackageToConsume"
               , out DtoOperation packageInitializeOperation1);

            // setup a simple db import operation, which initializes an operaion package in the taskContext:
            string packageInitializingQuery2 = @"select 'AAA' testJoinField, 'Works' as msg
union all 
select 'BBB', ' bad!'
union all 
select 'AAA', 'fine!';";
            CreateOperation(dtoTask.Id
               , 2
               , nameof(DbImporter)
               , packageInitializingQuery2
               , "PackageToConsume2"
               , out DtoOperation packageInitializeOperation2);

            //  setup a package consuming operation:
            string consumingQuery = @"select o.id, tt.msg
from [dbo].[Operation] o
join #RepPackPackageToConsume t on t.ConfigTemplate = o.config
join (select msg, testJoinField from #RepPackPackageToConsume2 where testJoinField != 'BBB') tt on o.config = tt.testJoinField";
            CreateOperation(dtoTask.Id
                , 3
                , nameof(DbPackageDataConsumer)
                , consumingQuery
                , "ConsumedPackage"
                , out DtoOperation packageConsumingOperation);

            SetupRepository(autofac, dtoTask, packageInitializeOperation1, packageInitializeOperation2, packageConsumingOperation);

            var logic = autofac.Resolve<ILogic>();
            logic.Start();
            logic.ForceExecute(dtoTask.Id);

            var taskContext = autofac.Resolve<IReportTaskRunContext>(); //  as it was registered as singleton during init.
            var packageParser = autofac.Resolve<IPackageParser>();
            while (taskContext.Packages.TryGetValue("ConsumedPackage", out _) == false)
            {
                await Task.Delay(1000); //  delay till package appears.
            }
            var commandText = autofac.Resolve<SqlCommandInitializer>().ResolveCommand().CommandText;

            Assert.Equal(
@$"CREATE TABLE #RepPackPackageToConsume ([id] INT NOT NULL,[ConfigTemplate] NVARCHAR(4000) NOT NULL)
INSERT INTO #RepPackPackageToConsume (""id"",""ConfigTemplate"")
VALUES(@p0,@p1)
INSERT INTO #RepPackPackageToConsume (""id"",""ConfigTemplate"")
VALUES(@p2,@p3)
INSERT INTO #RepPackPackageToConsume (""id"",""ConfigTemplate"")
VALUES(@p4,@p5)
INSERT INTO #RepPackPackageToConsume (""id"",""ConfigTemplate"")
VALUES(@p6,@p7)
CREATE TABLE #RepPackPackageToConsume2 ([testJoinField] NVARCHAR(4000) NOT NULL,[msg] NVARCHAR(4000) NOT NULL)
INSERT INTO #RepPackPackageToConsume2 (""testJoinField"",""msg"")
VALUES(@p8,@p9)
INSERT INTO #RepPackPackageToConsume2 (""testJoinField"",""msg"")
VALUES(@p10,@p11)
INSERT INTO #RepPackPackageToConsume2 (""testJoinField"",""msg"")
VALUES(@p12,@p13)
select o.id, tt.msg
from [dbo].[Operation] o
join #RepPackPackageToConsume t on t.ConfigTemplate = o.config
join (select msg, testJoinField from #RepPackPackageToConsume2 where testJoinField != 'BBB') tt on o.config = tt.testJoinField"
        , commandText);
        }

        [Fact]
        public async Task TestDbPackageConsumer()
        {
            IContainer autofac = InitializeTestContainer();
            await ConfigureDbForTestScenario();

            var dtoTask = new DtoTask();
            dtoTask.Id = 1;
            dtoTask.Name = $"{nameof(DbImporter)} - {nameof(DbPackageDataConsumer)} Test";

            // setup a simple db import operation, which initializes an operaion package in the taskContext:
            string packageInitializingQuery1 = @"select id, ConfigTemplate 
from [dbo].[OperTemplate]";
            CreateOperation(dtoTask.Id
               , 1
               , nameof(DbImporter)
               , packageInitializingQuery1
               , "PackageToConsume"
               , out DtoOperation packageInitializeOperation1);

            // setup a simple db import operation, which initializes an operaion package in the taskContext:
            string packageInitializingQuery2 = @"select 'AAA' testJoinField, 'Works' as msg
union all 
select 'BBB', ' bad!'
union all 
select 'AAA', 'fine!';";
            CreateOperation(dtoTask.Id
               , 2
               , nameof(DbImporter)
               , packageInitializingQuery2
               , "PackageToConsume2"
               , out DtoOperation packageInitializeOperation2);

            //  setup a package consuming operation:
            string consumingQuery = @"select o.id,  tt.msg
from[dbo].[Operation] o
join #RepPackPackageToConsume t on t.ConfigTemplate = o.config
join(select msg, testJoinField from #RepPackPackageToConsume2 where testJoinField != 'BBB') tt on o.config = tt.testJoinField";
            CreateOperation(dtoTask.Id
                , 3
                , nameof(DbPackageDataConsumer)
                , consumingQuery
                , "ConsumedPackage"
                , out DtoOperation packageConsumingOperation);

            SetupRepository(autofac, dtoTask, packageInitializeOperation1, packageInitializeOperation2, packageConsumingOperation);

            var logic = autofac.Resolve<ILogic>();
            logic.Start();
            logic.ForceExecute(dtoTask.Id);

            var taskContext = autofac.Resolve<IReportTaskRunContext>(); //  as it was registered as singleton during init.
            var packageParser = autofac.Resolve<IPackageParser>();
            while (taskContext.Packages.TryGetValue("ConsumedPackage", out _) == false)
            {
                await Task.Delay(1000); //  delay till package appears.
            }
            var consumeOperationResultedPackage = taskContext.Packages["ConsumedPackage"];
            Assert.NotNull(consumeOperationResultedPackage);
            Assert.True(packageParser.GetPackageValues(consumeOperationResultedPackage).Any());
            var dataSet = packageParser.GetPackageValues(consumeOperationResultedPackage)[0];
            var resultedString = string.Join(' ', dataSet.Rows.Select(x => x[1]));
            Assert.Equal("Works fine!", resultedString);
        }

        [Fact]
        public async void TestExceptionMessageOnFailedDbStructureCheck()
        {
            builder.RegisterType<B2BExporter>();
            builder.RegisterImplementation<IArchiver, ArchiverZip>();
            builder.RegisterImplementation<IReportTaskRunContext, ReportTaskRunContext>();

            RegisterExportConfig();
            RegisterDbStructureChecker();

            autofac = builder.Build();

            var b2bExport = autofac.Resolve<B2BExporter>();
            b2bExport.RunIfVoidPackage = true;

            var reportTaskContext = autofac.Resolve<IReportTaskRunContext>();
            reportTaskContext.CancelSource = new CancellationTokenSource();
            b2bExport.Properties.PackageName = nameof(b2bExport.Properties.PackageName);
            reportTaskContext.Packages.Add(b2bExport.Properties.PackageName, null);

            var ex = await Assert.ThrowsAnyAsync<Exception>(() => b2bExport.ExecuteAsync(reportTaskContext));

            var expectedExceptionMessage = string.Format(
                "The export database structure doesn't contain the data required for export. Required ExportTableName: {0}, ExportInstanceTableName: {1}."
                    , b2bExport.ExportTableName, b2bExport.ExportInstanceTableName);
            Assert.Equal(expectedExceptionMessage, ex.Message);
        }

        [Fact]
        public void TestFailedOperatinIsCorrectlyLoggedInDb()
        {
            ConfigureMonik(builder);
            ConfigureSqlServer();
            builder.RegisterImplementation<IArchiver, ArchiverZip>();
            builder.RegisterImplementation<IReportTaskRunContext, ReportTaskRunContext>();
            builder.RegisterImplementation<ITaskWorker, TaskWorker>();
            builder.RegisterImplementation<IDefaultTaskExporter, DefaultTaskExporter>();

            var autofac = builder.Build();
            var taskContext = autofac.Resolve<IReportTaskRunContext>();
            taskContext.CancelSource = new CancellationTokenSource();
            taskContext.TaskInstance = new DtoTaskInstance();
            taskContext.DependsOn = new List<TaskDependency>();
            var taskExporter = new Mock<IDefaultTaskExporter>();
            taskContext.DefaultExporter = taskExporter.Object;

            var operation = new Mock<IOperation>();
            var props = new CommonOperationProperties();
            operation.Setup(operation => operation.Properties).Returns(props);
            operation.Object.Properties.Number = 1;
            operation.Setup(operation => operation.ExecuteAsync(taskContext)).Returns(() => throw new NotImplementedException());

            taskContext.OpersToExecute = new List<IOperation>();
            taskContext.OpersToExecute.Add(operation.Object);

            string errMsg = string.Empty;
            var repository = autofac.Resolve<IRepository>();
            var repositoryMock = Mock.Get(repository);

            repositoryMock.Setup(m => m.UpdateEntity(It.IsAny<DtoOperInstance>()))
                .Callback<DtoOperInstance>((operInstance) => errMsg = operInstance.ErrorMessage);

            var worker = autofac.Resolve<ITaskWorker>();
            worker.RunTask(taskContext);
            repositoryMock.Verify(repo => repo.UpdateEntity(It.Is<DtoOperInstance>((s) => s.ErrorMessage.Equals(errMsg))));
        }
        #endregion


        #region private members

        private static void CreateOperation(
            long dtoTaskId,
            int operationNumber,
            string implementationType,
            string queryString,
            string packageName,
            out DtoOperation operation)
        {
            operation = new DtoOperation();
            operation.TaskId = dtoTaskId;
            operation.ImplementationType = implementationType;
            operation.Number = operationNumber;
            var operationConfig = new DbImporterConfig()
            {
                ConnectionString = TestDbConnStr,
                PackageName = packageName,
                Query = queryString,
                TimeOut = 10
            };
            operation.Config = JsonConvert.SerializeObject(operationConfig);
        }
        private IContainer InitializeTestContainer()
        {
            ConfigureMonik(builder);
            ConfigureSqlServer();
            ConfigureTelegramBot(builder);
            builder.RegisterImplementationSingleton<ILogic, Logic>();
            builder.RegisterImplementation<IArchiver, ArchiverZip>();
            builder.RegisterImplementationSingleton<IReportTaskRunContext, ReportTaskRunContext>();
            builder.RegisterImplementation<ITaskWorker, TaskWorker>();
            builder.RegisterImplementation<IDefaultTaskExporter, DefaultTaskExporter>();
            builder.RegisterImplementation<IPackageBuilder, ProtoPackageBuilder>();
            builder.RegisterImplementation<IPackageParser, ProtoPackageParser>();
            builder.RegisterImplementation<IReportTask, ReportTask.ReportTask>();
            var rnd = new ThreadSafeRandom();
            builder.RegisterSingleInstance<ThreadSafeRandom, ThreadSafeRandom>(rnd);
            var confRoot = new ConfigurationRoot(new List<IConfigurationProvider>());
            builder.RegisterSingleInstance<IConfigurationRoot, ConfigurationRoot>(confRoot);
            builder.RegisterType<DbPackageExportScriptCreator>();
            builder.RegisterType<SqlCommandInitializer>().SingleInstance();

            builder.RegisterType<DbPackageDataConsumer>()
                .Named<IOperation>(nameof(DbPackageDataConsumer))
                .Keyed<IOperation>(typeof(DbImporterConfig)).SingleInstance();

            builder.RegisterType<DbImporter>()
                .Named<IOperation>(nameof(DbImporter))
                .Keyed<IOperation>(typeof(DbImporterConfig));

            builder.RegisterType<Worker>();
            RegisterNamedViewExecutor<CommonTableViewExecutor>
                (builder, "CommonTableViewEx");

            var autofac = builder.Build();
            return autofac;
        }
        private void ConfigureSqlServer()
        {
            var repoMock = new Mock<IRepository>();
            builder.RegisterInstance(repoMock.Object).As<IRepository>().SingleInstance();
        }
        private static void SetupRepository(IContainer autofac, DtoTask dtoTask, DtoOperation packageInitializeOperation1, DtoOperation packageInitializeOperation2, DtoOperation packageConsumingOperation)
        {
            var repo = Mock.Get(autofac.Resolve<IRepository>());
            repo.Setup(r => r.UpdateOperInstancesAndGetIds()).Returns(new List<long>());
            repo.Setup(r => r.UpdateTaskInstancesAndGetIds()).Returns(new List<long>());
            repo.Setup(r => r.GetListEntitiesByDtoType<DtoTask>()).Returns(
                new List<DtoTask>(new[] { dtoTask }));
            repo.Setup(r => r.GetListEntitiesByDtoType<DtoOperation>()).Returns(
                new List<DtoOperation>(new[] {
                    packageInitializeOperation1,
                    packageInitializeOperation2,
                    packageConsumingOperation }));
            repo.Setup(r => r.GetTaskStateById(It.IsAny<long>())).Returns(new TaskState());
        }
        private void RegisterDbStructureChecker()
        {
            var structureChecker = new Mock<B2BDbStructureChecker>();
            structureChecker.Setup(checker => checker.CheckIfDbStructureExists(null, null)).Returns(Task.Factory.StartNew(() => false));
            builder.RegisterInstance(structureChecker.Object).As<B2BDbStructureChecker>();
        }
        private void RegisterExportConfig()
        {
            var exportConfig = new Mock<B2BExporterConfig>();
            exportConfig.Object.ExportInstanceTableName = nameof(exportConfig.Object.ExportInstanceTableName);
            exportConfig.Object.ExportTableName = nameof(exportConfig.Object.ExportTableName);
            builder.RegisterInstance(exportConfig.Object);
        }
        private void ConfigureMapper(ContainerBuilder builder)
        {
            builder.RegisterType<MapperProfile>().As<Profile>().SingleInstance();

            builder.Register(c =>
            {
                var profiles = c.Resolve<IEnumerable<Profile>>();

                var mapperConfig =
                    new MapperConfiguration(cfg =>
                    {
                        foreach (var prof in profiles)
                            cfg.AddProfile(prof);
                    });

                return mapperConfig.CreateMapper();
            })
                .As<IMapper>()
                .SingleInstance();
        }
        private void ConfigureMonik(ContainerBuilder builder)
        {
            var monikMock = new Mock<IMonik>();
            builder.RegisterInstance(monikMock.Object).As<IMonik>().SingleInstance();
        }
        private void ConfigureTelegramBot(ContainerBuilder builder)
        {
            var tg = new Mock<ITelegramBotClient>();
            builder.RegisterInstance<ITelegramBotClient>(tg.Object);

        }
        private void RegisterNamedViewExecutor<TImplementation>(ContainerBuilder builder, string name) where TImplementation : IViewExecutor
        {
            builder
                .RegisterType<TImplementation>()
                .Named<IViewExecutor>(name);
        }
        private async Task ConfigureDbForTestScenario()
        {
            using var connection = new SqlConnection(TestDbConnStr);
            {
                connection.Open();
                var cmd = connection.CreateCommand();
                cmd.CommandText = $@"
IF NOT EXISTS(SELECT * FROM sys.databases WHERE name = '{ReportServerTestDbName}')
BEGIN
    CREATE DATABASE[{ReportServerTestDbName}]
END";
                await cmd.ExecuteNonQueryAsync();
                var msCreator = new SqlServerRepository(null, null);
                msCreator.CreateBase(TestDbConnStr);

                connection.Execute(@"delete from [dbo].[OperInstance]");
                connection.Execute(@"delete from [dbo].[OperTemplate];");
                connection.Execute(@"delete from [dbo].[Operation];");
                connection.Execute(@"delete from [dbo].[TaskInstance];");
                connection.Execute(@"delete from [dbo].[Task]");

                connection.Execute(@"
set IDENTITY_INSERT [dbo].[Task] ON;
insert into [dbo].[Task] ([Id], [Name], [ScheduleId], [Parameters], [DependsOn], [UpdateDateTime])values
(0, 'TestTask', null, null , null, getDate());
insert into [dbo].[Task] ([Id], [Name], [ScheduleId], [Parameters], [DependsOn], [UpdateDateTime])values
(1, 'TestTask', null, null , null, getDate());
insert into [dbo].[Task] ([Id], [Name], [ScheduleId], [Parameters], [DependsOn], [UpdateDateTime])values
(2, 'TestTask', null, null , null, getDate());
insert into [dbo].[Task] ([Id], [Name], [ScheduleId], [Parameters], [DependsOn], [UpdateDateTime])values
(3, 'TestTask', null, null , null, getDate());");

                connection.Execute(@"
set IDENTITY_INSERT [dbo].[Task] ON;
insert into [dbo].[OperTemplate]
([ImplementationType], [Name], [ConfigTemplate])
select 'TestType', 'TestName', 'AAA'
union all
select 'TestType', 'TestName', 'BBB'
union all
select 'TestType', 'TestName', 'CCC'
union all
select 'TestType', 'TestName', 'DDD'");

                connection.Execute(@"
insert into [dbo].[Operation]
([TaskId], [Number], [IsDefault], [Config], [ImplementationType], [IsDeleted], [Name])
select '0', '0', '0', 'AAA', 'Test', '0', 'Test'
union all
select '1', '0', '0', 'BBB', 'Test', '0', 'Test'
union all
select '2', '0', '0', 'CCC', 'Test', '0', 'Test'
union all
select '3', '0', '0', 'DD', 'Test', '0', 'Test'
");
            }
            #endregion
        }
    }
}