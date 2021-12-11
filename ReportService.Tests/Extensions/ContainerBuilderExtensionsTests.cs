using System.Linq;
using System.Threading.Tasks;
using Autofac;
using Autofac.Core.Lifetime;
using NUnit.Framework;
using OfficeOpenXml;
using ReportService.Entities;
using ReportService.Extensions;
using ReportService.Interfaces.Core;
using ReportService.Interfaces.Operations;
using ReportService.Interfaces.ReportTask;
using Shouldly;

namespace ReportService.Tests.Extensions
{
    [TestFixture]
    public class ContainerBuilderExtensionsTests
    {
        [Test]
        public void RegisterImplementationSingleton_ShouldWorkProperly()
        {
            //Arrange
            var builder = new ContainerBuilder();
            var expectedLifetime = typeof(RootScopeLifetime);
            var expectedRegistrationType = typeof(TestClass);

            //Act
            builder.RegisterImplementationSingleton<ITestInterface, TestClass>();
            IContainer container = builder.Build();

            //Assert
            container.ComponentRegistry.Registrations.Single(registration => registration.Activator.LimitType == expectedRegistrationType).Lifetime.GetType()
                .ShouldBe(expectedLifetime);
        }

        [Test]
        public void RegisterImplementation_ShouldWorkProperly()
        {
            //Arrange
            var builder = new ContainerBuilder();
            var expectedLifetime = typeof(CurrentScopeLifetime);
            var expectedRegistrationType = typeof(TestClass);

            //Act
            builder.RegisterImplementation<ITestInterface, TestClass>();
            IContainer container = builder.Build();

            //Assert
            container.ComponentRegistry.Registrations.Single(registration => registration.Activator.LimitType == expectedRegistrationType).Lifetime.GetType()
                .ShouldBe(expectedLifetime);
        }

        [Test]
        public void RegisterInstance_ShouldWorkProperly()
        {
            //Arrange
            var builder = new ContainerBuilder();
            var expectedLifetime = typeof(CurrentScopeLifetime);
            var expectedRegistrationType = typeof(TestClass);
            var expectedInstance = new TestClass();

            //Act
            builder.RegisterInstance<ITestInterface, TestClass>(expectedInstance);
            IContainer container = builder.Build();

            //Assert
            container.ComponentRegistry.Registrations.Single(registration => registration.Activator.LimitType == expectedRegistrationType).Lifetime.GetType()
                .ShouldBe(expectedLifetime);
            container.Resolve<ITestInterface>().ShouldBe(expectedInstance);
        }

        [Test]
        public void RegisterSingleInstance_ShouldWorkProperly()
        {
            //Arrange
            var builder = new ContainerBuilder();
            var expectedLifetime = typeof(RootScopeLifetime);
            var expectedRegistrationType = typeof(TestClass);
            var expectedInstance = new TestClass();

            //Act
            builder.RegisterSingleInstance<ITestInterface, TestClass>(expectedInstance);
            IContainer container = builder.Build();

            //Assert
            container.ComponentRegistry.Registrations.Single(registration => registration.Activator.LimitType == expectedRegistrationType).Lifetime.GetType()
                .ShouldBe(expectedLifetime);
            container.Resolve<ITestInterface>().ShouldBe(expectedInstance);
        }

        [Test]
        public void RegisterNamedSingleton_ShouldWorkProperly()
        {
            //Arrange
            var builder = new ContainerBuilder();
            var expectedLifetime = typeof(RootScopeLifetime);
            var expectedRegistrationType = typeof(TestClass2);
            var registrationName = "TestRegistration2";

            //Act
            builder.RegisterNamedSingleton<ITestInterface, TestClass>("TestRegistration");
            builder.RegisterNamedSingleton<ITestInterface, TestClass2>(registrationName);
            IContainer container = builder.Build();

            //Assert
            container.ComponentRegistry.Registrations.Single(registration => registration.Activator.LimitType == expectedRegistrationType).Lifetime.GetType()
                .ShouldBe(expectedLifetime);

            container.ResolveNamed<ITestInterface>(registrationName).ShouldBeOfType(expectedRegistrationType);
        }

        [Test]
        public void RegisterNamedImplementation_ShouldWorkProperly()
        {
            //Arrange
            var builder = new ContainerBuilder();
            var expectedLifetime = typeof(CurrentScopeLifetime);
            var expectedRegistrationType = typeof(TestClass2);
            var registrationName = "TestRegistration2";

            //Act
            builder.RegisterNamedImplementation<ITestInterface, TestClass>("TestRegistration");
            builder.RegisterNamedImplementation<ITestInterface, TestClass2>(registrationName);
            IContainer container = builder.Build();

            //Assert
            container.ComponentRegistry.Registrations.Single(registration => registration.Activator.LimitType == expectedRegistrationType).Lifetime.GetType()
                .ShouldBe(expectedLifetime);

            container.ResolveNamed<ITestInterface>(registrationName).ShouldBeOfType(expectedRegistrationType);
        }

        [Test]
        public void RegisterNamedDataExporter_ShouldWorkProperly()
        {
            //Arrange
            var builder = new ContainerBuilder();
            var expectedLifetime = typeof(CurrentScopeLifetime);
            var expectedRegistrationType = typeof(TestOperation);
            var expectedKey = typeof(TestExporterConfig);
            var registrationName = "TestOperationName";

            //Act
            builder.RegisterNamedDataExporter<TestOperation, TestExporterConfig>(registrationName);
            IContainer container = builder.Build();

            //Assert
            container.ComponentRegistry.Registrations.Single(registration => registration.Activator.LimitType == expectedRegistrationType).Lifetime.GetType()
                .ShouldBe(expectedLifetime);

            container.ResolveNamed<IOperation>(registrationName).ShouldBeOfType(expectedRegistrationType);
            container.ResolveKeyed<IOperation>(expectedKey).ShouldBeOfType(expectedRegistrationType);
        }

        [Test]
        public void RegisterNamedDataImporter_ShouldWorkProperly()
        {
            //Arrange
            var builder = new ContainerBuilder();
            var expectedLifetime = typeof(CurrentScopeLifetime);
            var expectedRegistrationType = typeof(TestOperation);
            var expectedKey = typeof(TestImporterConfig);
            var registrationName = "TestOperationName";

            //Act
            builder.RegisterNamedDataImporter<TestOperation, TestImporterConfig>(registrationName);
            IContainer container = builder.Build();

            //Assert
            container.ComponentRegistry.Registrations.Single(registration => registration.Activator.LimitType == expectedRegistrationType).Lifetime.GetType()
                .ShouldBe(expectedLifetime);

            container.ResolveNamed<IOperation>(registrationName).ShouldBeOfType(expectedRegistrationType);
            container.ResolveKeyed<IOperation>(expectedKey).ShouldBeOfType(expectedRegistrationType);
        }

        [Test]
        public void RegisterNamedViewExecutor_ShouldWorkProperly()
        {
            //Arrange
            var builder = new ContainerBuilder();
            var expectedLifetime = typeof(CurrentScopeLifetime);
            var expectedRegistrationType = typeof(TestViewExecutor);
            var registrationName = "TestViewExecutorName";

            //Act
            builder.RegisterNamedViewExecutor<TestViewExecutor>(registrationName);
            IContainer container = builder.Build();

            //Assert
            container.ComponentRegistry.Registrations.Single(registration => registration.Activator.LimitType == expectedRegistrationType).Lifetime.GetType()
                .ShouldBe(expectedLifetime);

            container.ResolveNamed<IViewExecutor>(registrationName).ShouldBeOfType(expectedRegistrationType);
        }

        private interface ITestInterface { }

        private class TestClass: ITestInterface { }
        private class TestClass2: ITestInterface { }

        private class TestOperation : IOperation
        {
            public CommonOperationProperties Properties { get; set; }
            public bool CreateDataFolder { get; set; }
            public Task ExecuteAsync(IReportTaskRunContext taskContext)
            {
                throw new System.NotImplementedException();
            }
        }

        private class TestExporterConfig : IExporterConfig
        {
            public string PackageName { get; set; }
            public bool RunIfVoidPackage { get; set; }
        }

        private class TestImporterConfig : IImporterConfig
        {
            public string PackageName { get; set; }
        }

        private class TestViewExecutor : IViewExecutor
        {
            public string ExecuteHtml(string viewTemplate, OperationPackage package)
            {
                throw new System.NotImplementedException();
            }

            public string ExecuteTelegramView(OperationPackage package, string reportName = "Report", bool useAllSets = false)
            {
                throw new System.NotImplementedException();
            }

            public ExcelPackage ExecuteXlsx(OperationPackage package, string reportName, bool useAllSets = false)
            {
                throw new System.NotImplementedException();
            }

            public byte[] ExecuteCsv(OperationPackage package, string delimiter = ";", bool useAllSets = false)
            {
                throw new System.NotImplementedException();
            }
        }
    }
}
