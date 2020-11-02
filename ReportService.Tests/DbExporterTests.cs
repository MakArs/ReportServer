using Autofac;
using AutoMapper;
using Monik.Client;
using Monik.Common;
using Moq;
using ReportService.Core;
using ReportService.Entities;
using ReportService.Entities.Dto;
using ReportService.Extensions;
using ReportService.Interfaces.Core;
using ReportService.Interfaces.Operations;
using ReportService.Interfaces.ReportTask;
using ReportService.Operations.DataExporters;
using ReportService.Operations.DataExporters.Configurations;
using ReportService.Operations.DataExporters.Dependencies;
using ReportService.ReportTask;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace ReportService.Tests
{

    public class DbExporterTests
    {

        #region fields

        private ILifetimeScope autofac;
        private Mock<IMonik> monikMock;
        private readonly ContainerBuilder builder;
        #endregion


        #region ctor

        public DbExporterTests()
        {
            builder = new ContainerBuilder();
            ConfigureMapper(builder);
        }
        #endregion


        #region testing methods

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
            monikMock = new Mock<IMonik>();
            builder.RegisterInstance(monikMock.Object).As<IMonik>().SingleInstance();
        }
        #endregion
    }
}