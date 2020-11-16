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
using System.Net;
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
        private const string WorkTableName = "TestTable";
        private const string TestDbConnStr = "Data Source=WS-00245;Database=ReportsSchemeForUnitTesting;Integrated Security=true";
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
        public void TestDbPackageConsumer()
        {
            IContainer autofac = InitializeTestContainer();

            var dtoTask = new Mock<DtoTask>();
            dtoTask.Object.Id = 1;
            dtoTask.Object.Name = $"{nameof(DbImporter)} - {nameof(DbPackageDataConsumer)} Test";

            // simple import from db to package operation:
            var dbPackageInitializeDtoOperation = new Mock<DtoOperation>();
            dbPackageInitializeDtoOperation.Object.TaskId = dtoTask.Object.Id;
            dbPackageInitializeDtoOperation.Object.ImplementationType = nameof(DbImporter);
            dbPackageInitializeDtoOperation.Object.Number = 1;
            var dbPackageInitializerConfig = new DbImporterConfig()
            {
                PackageName = "PackageToConsume",
                ConnectionString = TestDbConnStr,
                Query =
@"select id, ConfigTemplate 
from [dbo].[OperTemplate]",
                TimeOut = 10,
            };
            dbPackageInitializeDtoOperation.Object.Config = JsonConvert.SerializeObject(dbPackageInitializerConfig);

            
            // simple import from db to package operation:
            var dbPackageInitializeDtoOperation2 = new Mock<DtoOperation>();
            dbPackageInitializeDtoOperation2.Object.TaskId = dtoTask.Object.Id;
            dbPackageInitializeDtoOperation2.Object.ImplementationType = nameof(DbImporter);
            dbPackageInitializeDtoOperation2.Object.Number = 2;
            var dbPackageInitializerConfig2 = new DbImporterConfig()
            {
                PackageName = "PackageToConsume2",
                ConnectionString = TestDbConnStr,
                Query =
@"select 'AAA' testJoinField, 'Works' as msg
union all 
select 'BBB', ' bug'
union all 
select 'AAA', 'fine!';",
                TimeOut = 10,
            };
            dbPackageInitializeDtoOperation2.Object.Config = JsonConvert.SerializeObject(dbPackageInitializerConfig2);

            var dbPackageConsumingOperation = new Mock<DtoOperation>();
            dbPackageConsumingOperation.Object.TaskId = dtoTask.Object.Id;
            dbPackageConsumingOperation.Object.ImplementationType = nameof(DbPackageDataConsumer);
            dbPackageConsumingOperation.Object.Number = 3;
            var dbDataConsumerConfig = new DbImporterConfig()
            {
                ConnectionString = TestDbConnStr,
                PackageName = "ConsumedPackage",
                Query = $@"
select o.id,  tt.msg
from [dbo].[Operation] o
join #RepPackPackageToConsume t on t.ConfigTemplate = o.config
join (select msg, testJoinField from #RepPackPackageToConsume2 where testJoinField != 'BBB') tt on o.config = tt.testJoinField",
                TimeOut = 10
            };
            dbPackageConsumingOperation.Object.Config = JsonConvert.SerializeObject(dbDataConsumerConfig);

            var repoMock = autofac.Resolve<IRepository>();
            var repo = Mock.Get(repoMock);
            ConfigureDbForTestScenario();
            repo.Setup(r => r.UpdateOperInstancesAndGetIds()).Returns(new List<long>());
            repo.Setup(r => r.UpdateTaskInstancesAndGetIds()).Returns(new List<long>());
            repo.Setup(r => r.GetListEntitiesByDtoType<DtoTask>()).Returns(
                new List<DtoTask>(new[] { dtoTask.Object }));
            repo.Setup(r => r.GetListEntitiesByDtoType<DtoOperation>()).Returns(
                new List<DtoOperation>(new[] {
                    dbPackageInitializeDtoOperation.Object,
                    dbPackageInitializeDtoOperation2.Object,
                    dbPackageConsumingOperation.Object }));
            repo.Setup(r => r.GetTaskStateById(It.IsAny<long>())).Returns(new TaskState());

            var logic = autofac.Resolve<ILogic>();
            logic.Start();
            logic.ForceExecute(dtoTask.Object.Id);


            var taskContext = autofac.Resolve<IReportTaskRunContext>(); //  as it was registered as singleton during init.
            var packageParaser = autofac.Resolve<IPackageParser>();
            var consumeOperationResultedPackage = taskContext.Packages[dbDataConsumerConfig.PackageName];
            Assert.NotNull(consumeOperationResultedPackage);
            var dataSet = packageParaser.GetPackageValues(consumeOperationResultedPackage)[0];
            var resultedString = string.Join(' ', dataSet.Rows.Select(x => x[1]));
            Assert.Equal("Works fine!", resultedString);
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
            builder.RegisterType<DbPackageExportScriptCreator>().SingleInstance();

            builder.RegisterType<DbPackageDataConsumer>()
                .Named<IOperation>(nameof(DbPackageDataConsumer))
                .Keyed<IOperation>(typeof(DbImporterConfig));

            builder.RegisterType<DbImporter>()
                .Named<IOperation>(nameof(DbImporter))
                .Keyed<IOperation>(typeof(DbImporterConfig));

            builder.RegisterType<Worker>();
            RegisterNamedViewExecutor<CommonTableViewExecutor>
                (builder, "CommonTableViewEx");

            var autofac = builder.Build();
            return autofac;
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
            builder.RegisterImplementation<IDefaultTaskExporter,DefaultTaskExporter>();

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

        private void ConfigureSqlServer()
        {
            var repoMock = new Mock<IRepository>();
            builder.RegisterInstance(repoMock.Object).As<IRepository>().SingleInstance();
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
        
        private void ConfigureDbForTestScenario()
        {
            var msCreator = new SqlServerRepository(null, null);
            msCreator.CreateBase(TestDbConnStr);

            using var connection = new SqlConnection(TestDbConnStr);

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