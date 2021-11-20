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
using ReportService.Protobuf;
using ReportService.ReportTask;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using ReportService.Operations.DataImporters.Helpers;
using Telegram.Bot;

using IConfigurationProvider = Microsoft.Extensions.Configuration.IConfigurationProvider;

namespace ReportService.Tests
{
    [TestFixture]
    [NonParallelizable]
    public class DbExporterTests
    {

        private ILifetimeScope mContainer;
        private ContainerBuilder mBuilder;
        private const string cReportServerTestDbName = "ReportServerTestDb";
        private const string cTestDbConnStr = "Server=localhost,1438;User Id=sa;Password=P@55w0rd;Timeout=5";

        [SetUp]
        public void SetUpClass()
        {
            mBuilder = new ContainerBuilder();
            ConfigureMapper(mBuilder);
        }

        [Ignore("Should make less fragile")]
        [Test]
        public async Task TestExportPackageScriptCreator()
        {
            IContainer container = InitializeTestContainer();
            await ConfigureDbForTestScenario();

            var dtoTask = new DtoTask
            {
                Id = 1,
                Name = $"{nameof(DbImporter)} - {nameof(DbPackageDataConsumer)} Test"
            };

            // setup a simple db import operation, which initializes an operation package in the taskContext:
            string packageInitializingQuery1 = @"select id, ConfigTemplate 
                    from [dbo].[OperTemplate]";

            CreateOperation(dtoTask.Id, 1, nameof(DbImporter), packageInitializingQuery1, "PackageToConsume", 
                out DtoOperation packageInitializeOperation1);

            // setup a simple db import operation, which initializes an operation package in the taskContext:
            string packageInitializingQuery2 = @"select 'AAA' testJoinField, 'Works' as msg
                    union all 
                    select 'BBB', ' bad!'
                    union all 
                    select 'AAA', 'fine!';";

            CreateOperation(dtoTask.Id, 2, nameof(DbImporter), packageInitializingQuery2, "PackageToConsume2", 
                out DtoOperation packageInitializeOperation2);

            //  setup a package consuming operation:
            string consumingQuery = @"select o.id, tt.msg
                    from [dbo].[Operation] o
                    join #RepPackPackageToConsume t on t.ConfigTemplate = o.config
                    join (select msg, testJoinField from #RepPackPackageToConsume2 where testJoinField != 'BBB') tt on o.config = tt.testJoinField";

            CreateOperation(dtoTask.Id, 3, nameof(DbPackageDataConsumer), consumingQuery, "ConsumedPackage", 
                out DtoOperation packageConsumingOperation);

            SetupRepository(container, dtoTask, packageInitializeOperation1, packageInitializeOperation2, packageConsumingOperation);

            var logic = container.Resolve<ILogic>();
            logic.Start();
            logic.ForceExecute(dtoTask.Id);

            var taskContext = container.Resolve<IReportTaskRunContext>(); //  as it was registered as singleton during init.

            while (taskContext.Packages.TryGetValue("ConsumedPackage", out _) == false)
            {
                await Task.Delay(1000); //  delay till package appears.
            }

            var commandText = container.Resolve<SqlCommandInitializer>().ResolveCommand().CommandText;

            Assert.AreEqual(@"CREATE TABLE #RepPackPackageToConsume ([id] INT NOT NULL,[ConfigTemplate] NVARCHAR(4000) NOT NULL)
                    INSERT INTO #RepPackPackageToConsume (""id"",""ConfigTemplate"") VALUES(@p0,@p1)
                    INSERT INTO #RepPackPackageToConsume (""id"",""ConfigTemplate"") VALUES(@p2,@p3)
                    INSERT INTO #RepPackPackageToConsume (""id"",""ConfigTemplate"") VALUES(@p4,@p5)
                    INSERT INTO #RepPackPackageToConsume (""id"",""ConfigTemplate"") VALUES(@p6,@p7)
                    CREATE TABLE #RepPackPackageToConsume2 ([testJoinField] NVARCHAR(4000) NOT NULL,[msg] NVARCHAR(4000) NOT NULL)
                    INSERT INTO #RepPackPackageToConsume2 (""testJoinField"",""msg"") VALUES(@p8,@p9)
                    INSERT INTO #RepPackPackageToConsume2 (""testJoinField"",""msg"") VALUES(@p10,@p11)
                    INSERT INTO #RepPackPackageToConsume2 (""testJoinField"",""msg"") VALUES(@p12,@p13)
                    select o.id, tt.msg
                    from [dbo].[Operation] o
                    join #RepPackPackageToConsume t on t.ConfigTemplate = o.config
                    join (select msg, testJoinField from #RepPackPackageToConsume2 where testJoinField != 'BBB') tt on o.config = tt.testJoinField", 
                commandText);
        }

        [Test]
        public async Task TestDbPackageConsumer()
        {
            IContainer container = InitializeTestContainer();
            await ConfigureDbForTestScenario();

            var dtoTask = new DtoTask
            {
                Id = 1,
                Name = $"{nameof(DbImporter)} - {nameof(DbPackageDataConsumer)} Test"
            };

            // setup a simple db import operation, which initializes an operation package in the taskContext:
            string packageInitializingQuery1 = @"select id, ConfigTemplate 
                    from [dbo].[OperTemplate]";

            CreateOperation(dtoTask.Id, 1, nameof(DbImporter), packageInitializingQuery1, "PackageToConsume", 
                out DtoOperation packageInitializeOperation1);

            // setup a simple db import operation, which initializes an operation package in the taskContext:
            string packageInitializingQuery2 = @"select 'AAA' testJoinField, 'Works' as msg
                    union all 
                    select 'BBB', ' bad!'
                    union all 
                    select 'AAA', 'fine!';";

            CreateOperation(dtoTask.Id, 2, nameof(DbImporter), packageInitializingQuery2, "PackageToConsume2", 
                out DtoOperation packageInitializeOperation2);

            //  setup a package consuming operation:
            string consumingQuery = @"select o.id,  tt.msg
                    from[dbo].[Operation] o
                    join #RepPackPackageToConsume t on t.ConfigTemplate = o.config
                    join(select msg, testJoinField from #RepPackPackageToConsume2 where testJoinField != 'BBB') tt on o.config = tt.testJoinField";
            
            CreateOperation(dtoTask.Id, 3, nameof(DbPackageDataConsumer), consumingQuery, "ConsumedPackage", 
                out DtoOperation packageConsumingOperation);

            SetupRepository(container, dtoTask, packageInitializeOperation1, packageInitializeOperation2, packageConsumingOperation);

            var logic = container.Resolve<ILogic>();
            logic.Start();
            logic.ForceExecute(dtoTask.Id);

            var taskContext = container.Resolve<IReportTaskRunContext>(); //  as it was registered as singleton during init.
            var packageParser = container.Resolve<IPackageParser>();

            while (taskContext.Packages.TryGetValue("ConsumedPackage", out _) == false)
            {
                await Task.Delay(1000); //  delay till package appears.
            }

            var consumeOperationResultedPackage = taskContext.Packages["ConsumedPackage"];
            Assert.NotNull(consumeOperationResultedPackage);
            Assert.True(packageParser.GetPackageValues(consumeOperationResultedPackage).Any());
            var dataSet = packageParser.GetPackageValues(consumeOperationResultedPackage)[0];
            var resultedString = string.Join(' ', dataSet.Rows.Select(x => x[1]));
            Assert.AreEqual("Works fine!", resultedString);
        }

        [Test]
        public void TestExceptionMessageOnFailedDbStructureCheck()
        {
            mBuilder.RegisterType<B2BExporter>();
            mBuilder.RegisterImplementation<IArchiver, ArchiverZip>();
            mBuilder.RegisterImplementation<IReportTaskRunContext, ReportTaskRunContext>();

            RegisterExportConfig();
            RegisterDbStructureChecker();

            mContainer = mBuilder.Build();

            var b2bExport = mContainer.Resolve<B2BExporter>();
            b2bExport.RunIfVoidPackage = true;

            var reportTaskContext = mContainer.Resolve<IReportTaskRunContext>();
            reportTaskContext.CancelSource = new CancellationTokenSource();
            b2bExport.Properties.PackageName = nameof(b2bExport.Properties.PackageName);
            reportTaskContext.Packages.Add(b2bExport.Properties.PackageName, null);

            var ex = Assert.ThrowsAsync<Exception>(() => b2bExport.ExecuteAsync(reportTaskContext));

            var expectedExceptionMessage = $"The export database structure doesn't contain the data required for export. Required ExportTableName: {b2bExport.ExportTableName}, " +
                                           $"ExportInstanceTableName: {b2bExport.ExportInstanceTableName}.";
            Assert.AreEqual(expectedExceptionMessage, ex.Message);
        }

        [Test]
        public void TestFailedOperationIsCorrectlyLoggedInDb()
        {
            ConfigureMonik(mBuilder);
            ConfigureSqlServer();
            mBuilder.RegisterImplementation<IArchiver, ArchiverZip>();
            mBuilder.RegisterImplementation<IReportTaskRunContext, ReportTaskRunContext>();
            mBuilder.RegisterImplementation<ITaskWorker, TaskWorker>();
            mBuilder.RegisterImplementation<IDefaultTaskExporter, DefaultTaskExporter>();

            var container = mBuilder.Build();
            var taskContext = container.Resolve<IReportTaskRunContext>();
            taskContext.CancelSource = new CancellationTokenSource();
            taskContext.TaskInstance = new DtoTaskInstance();
            taskContext.DependsOn = new List<TaskDependency>();
            var taskExporter = new Mock<IDefaultTaskExporter>();
            taskContext.DefaultExporter = taskExporter.Object;

            var operationMock = new Mock<IOperation>();
            var props = new CommonOperationProperties();
            operationMock.Setup(operation => operation.Properties).Returns(props);
            operationMock.Object.Properties.Number = 1;
            operationMock.Setup(operation => operation.ExecuteAsync(taskContext)).Returns(() => throw new NotImplementedException());

            taskContext.OpersToExecute = new List<IOperation>
            {
                operationMock.Object
            };

            string errMsg = string.Empty;
            var repository = container.Resolve<IRepository>();
            var repositoryMock = Mock.Get(repository);

            repositoryMock.Setup(m => m.UpdateEntity(It.IsAny<DtoOperInstance>()))
                .Callback<DtoOperInstance>(operInstance => errMsg = operInstance.ErrorMessage);

            var worker = container.Resolve<ITaskWorker>();
            worker.RunTask(taskContext);
            repositoryMock.Verify(repo => repo.UpdateEntity(It.Is<DtoOperInstance>((s) => s.ErrorMessage.Equals(errMsg))));
        }

        private static void CreateOperation(long dtoTaskId, int operationNumber, string implementationType, string queryString,
            string packageName, out DtoOperation operation)
        {
            operation = new DtoOperation
            {
                TaskId = dtoTaskId,
                ImplementationType = implementationType,
                Number = operationNumber
            };

            var operationConfig = new DbImporterConfig()
            {
                ConnectionString = cTestDbConnStr,
                PackageName = packageName,
                Query = queryString,
                TimeOut = 10
            };

            operation.Config = JsonConvert.SerializeObject(operationConfig);
        }

        private IContainer InitializeTestContainer()
        {
            ConfigureMonik(mBuilder);
            ConfigureSqlServer();
            ConfigureTelegramBot(mBuilder);
            mBuilder.RegisterImplementationSingleton<ILogic, Logic>();
            mBuilder.RegisterImplementation<IArchiver, ArchiverZip>();
            mBuilder.RegisterImplementationSingleton<IReportTaskRunContext, ReportTaskRunContext>();
            mBuilder.RegisterImplementation<ITaskWorker, TaskWorker>();
            mBuilder.RegisterImplementation<IDefaultTaskExporter, DefaultTaskExporter>();
            mBuilder.RegisterImplementation<IPackageBuilder, ProtoPackageBuilder>();
            mBuilder.RegisterImplementation<IPackageParser, ProtoPackageParser>();
            mBuilder.RegisterImplementation<IReportTask, ReportTask.ReportTask>();
            var rnd = new ThreadSafeRandom();
            mBuilder.RegisterSingleInstance<ThreadSafeRandom, ThreadSafeRandom>(rnd);
            var confRoot = new ConfigurationRoot(new List<IConfigurationProvider>());
            mBuilder.RegisterSingleInstance<IConfigurationRoot, ConfigurationRoot>(confRoot);
            mBuilder.RegisterType<DbPackageExportScriptCreator>();
            mBuilder.RegisterType<SqlCommandInitializer>().SingleInstance();

            mBuilder.RegisterType<DbPackageDataConsumer>()
                .Named<IOperation>(nameof(DbPackageDataConsumer))
                .Keyed<IOperation>(typeof(DbImporterConfig)).SingleInstance();

            mBuilder.RegisterType<DbImporter>()
                .Named<IOperation>(nameof(DbImporter))
                .Keyed<IOperation>(typeof(DbImporterConfig));

            mBuilder.RegisterType<Worker>();
            RegisterNamedViewExecutor<CommonTableViewExecutor>
                (mBuilder, "CommonTableViewEx");

            var container = mBuilder.Build();
            return container;
        }

        private void ConfigureSqlServer()
        {
            var repoMock = new Mock<IRepository>();
            mBuilder.RegisterInstance(repoMock.Object).As<IRepository>().SingleInstance();
        }

        private static void SetupRepository(IContainer container, DtoTask dtoTask, DtoOperation packageInitializeOperation1, 
            DtoOperation packageInitializeOperation2, DtoOperation packageConsumingOperation)
        {
            var repo = Mock.Get(container.Resolve<IRepository>());
            repo.Setup(r => r.UpdateOperInstancesAndGetIds()).Returns(new List<long>());
            repo.Setup(r => r.UpdateTaskInstancesAndGetIds()).Returns(new List<long>());
            repo.Setup(r => r.GetListEntitiesByDtoType<DtoTask>()).Returns(new List<DtoTask>(new[] { dtoTask }));

            repo.Setup(r => r.GetListEntitiesByDtoType<DtoOperation>()).Returns(new List<DtoOperation>(new[]
                {
                    packageInitializeOperation1, packageInitializeOperation2, packageConsumingOperation
                }));
            repo.Setup(r => r.GetTaskStateById(It.IsAny<long>())).Returns(new TaskState());
        }

        private void RegisterDbStructureChecker()
        {
            var structureChecker = new Mock<B2BDBStructureChecker>();
            structureChecker
                .Setup(checker => checker.CheckIfDbStructureExists(null, null))
                .ReturnsAsync(false);
            mBuilder.RegisterInstance(structureChecker.Object).As<IDBStructureChecker>();
        }

        private void RegisterExportConfig()
        {
            var exportConfig = new Mock<B2BExporterConfig>();
            exportConfig.Object.ExportInstanceTableName = nameof(exportConfig.Object.ExportInstanceTableName);
            exportConfig.Object.ExportTableName = nameof(exportConfig.Object.ExportTableName);
            mBuilder.RegisterInstance(exportConfig.Object);
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
            builder.RegisterInstance(tg.Object);

        }

        private void RegisterNamedViewExecutor<TImplementation>(ContainerBuilder builder, string name) where TImplementation : IViewExecutor
        {
            builder
                .RegisterType<TImplementation>()
                .Named<IViewExecutor>(name);
        }

        private async Task ConfigureDbForTestScenario()
        {
            await using var connection = new SqlConnection(cTestDbConnStr);
            {
                connection.Open();
                var cmd = connection.CreateCommand();
                cmd.CommandText = $@"IF NOT EXISTS(SELECT * FROM sys.databases WHERE name = '{cReportServerTestDbName}')
                        BEGIN
                            CREATE DATABASE[{cReportServerTestDbName}]
                        END";
                await cmd.ExecuteNonQueryAsync();
                var msCreator = new SqlServerRepository(null, null);
                msCreator.CreateSchema(cTestDbConnStr);

                await connection.ExecuteAsync(@"delete from [dbo].[OperInstance]");
                await connection.ExecuteAsync(@"delete from [dbo].[OperTemplate];");
                await connection.ExecuteAsync(@"delete from [dbo].[Operation];");
                await connection.ExecuteAsync(@"delete from [dbo].[TaskInstance];");
                await connection.ExecuteAsync(@"delete from [dbo].[Task]");

                await connection.ExecuteAsync(@"set IDENTITY_INSERT [dbo].[Task] ON;
                        insert into [dbo].[Task] ([Id], [Name], [ScheduleId], [Parameters], [DependsOn], [UpdateDateTime])values
                        (0, 'TestTask', null, null , null, getDate());
                        insert into [dbo].[Task] ([Id], [Name], [ScheduleId], [Parameters], [DependsOn], [UpdateDateTime])values
                        (1, 'TestTask', null, null , null, getDate());
                        insert into [dbo].[Task] ([Id], [Name], [ScheduleId], [Parameters], [DependsOn], [UpdateDateTime])values
                        (2, 'TestTask', null, null , null, getDate());
                        insert into [dbo].[Task] ([Id], [Name], [ScheduleId], [Parameters], [DependsOn], [UpdateDateTime])values
                        (3, 'TestTask', null, null , null, getDate());");

                await connection.ExecuteAsync(@"set IDENTITY_INSERT [dbo].[Task] ON;
                        insert into [dbo].[OperTemplate]
                        ([ImplementationType], [Name], [ConfigTemplate])
                        select 'TestType', 'TestName', 'AAA'
                        union all
                        select 'TestType', 'TestName', 'BBB'
                        union all
                        select 'TestType', 'TestName', 'CCC'
                        union all
                        select 'TestType', 'TestName', 'DDD'");

                await connection.ExecuteAsync(@"insert into [dbo].[Operation]
                        ([TaskId], [Number], [IsDefault], [Config], [ImplementationType], [IsDeleted], [Name])
                        select '0', '0', '0', 'AAA', 'Test', '0', 'Test'
                        union all
                        select '1', '0', '0', 'BBB', 'Test', '0', 'Test'
                        union all
                        select '2', '0', '0', 'CCC', 'Test', '0', 'Test'
                        union all
                        select '3', '0', '0', 'DD', 'Test', '0', 'Test'");
            }
        }
    }
}
