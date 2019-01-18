using System;
using Autofac;
using Nancy.Hosting.Self;
using NUnit.Framework;
using ReportService;
using ReportService.Interfaces.Core;
using ReportService.Interfaces.ReportTask;
using ReportService.Operations.DataImporters;

namespace TestModule
{
    [TestFixture]
    public class Operations
    {
        private readonly ILifetimeScope autofac;

        public Operations()
        {
            HostConfiguration hostConfigs = new HostConfiguration()
            {
                UrlReservations = new UrlReservations {CreateAutomatically = true},
                RewriteLocalhost = true
            };

            var nancyHost = new NancyHost(
                new Uri("http://localhost:12345"),
                new Bootstrapper(),
                hostConfigs);

            autofac = Bootstrapper.Global.Resolve<ILifetimeScope>();
        }

        [Test]
        public void GetFileFromSshTest()
        {
            var sshImp = autofac.ResolveNamed<IOperation>("CommonSshImporter");
            var taskContext = autofac.Resolve<IRTaskRunContext>();
            var dtoTaskInstance = new DtoTaskInstance
            {
                Id=151256,
                StartTime = DateTime.Now,
                Duration = 0,
                State = (int)InstanceState.InProcess
            };
            taskContext.TaskInstance = dtoTaskInstance;
            sshImp.Execute(taskContext);
           
        }

        [Test]
        public void CsvImporterTest()
        {
            var csvimp = autofac.ResolveNamed<IOperation>("CommonCsvImporter",
                new NamedParameter("config", new CsvImporterConfig
                {
                    FilePath = @"C:\ArsMak\rep171302.csv",
                    PackageName = "package0101",
                    Delimiter = ";"
                }));

            var csvimp2 = autofac.ResolveNamed<IOperation>("CommonCsvImporter",
                new NamedParameter("config", new CsvImporterConfig
                {
                    FilePath = @"C:\ArsMak\rep171303.csv",
                    PackageName = "package0102",
                    Delimiter = ","
                }));

            var csvimp3 = autofac.ResolveNamed<IOperation>("CommonCsvImporter",
                new NamedParameter("config", new CsvImporterConfig
                {
                    FilePath = @"C:\ArsMak\rep171304.csv",
                    PackageName = "package0103",
                    Delimiter = "\\t"
                }));

            var csvimp4 = autofac.ResolveNamed<IOperation>("CommonCsvImporter",
                new NamedParameter("config", new CsvImporterConfig
                {
                    FilePath = @"C:\ArsMak\rep171305.csv",
                    PackageName = "package0104",
                    Delimiter = "\\r\\n"
                }));

            var csvimp5 = autofac.ResolveNamed<IOperation>("CommonCsvImporter",
                new NamedParameter("config", new CsvImporterConfig
                {
                    FilePath = @"C:\ArsMak\rep171306.csv",
                    PackageName = "package0105",
                    Delimiter = "|"
                }));

            var csvimp6 = autofac.ResolveNamed<IOperation>("CommonCsvImporter",
                new NamedParameter("config", new CsvImporterConfig
                {
                    FilePath = @"C:\ArsMak\rep171307.csv",
                    PackageName = "package0106",
                    Delimiter = "."
                }));

            var taskContext = autofac.Resolve<IRTaskRunContext>();

            csvimp.Execute(taskContext);
            csvimp2.Execute(taskContext);
            csvimp3.Execute(taskContext);
            csvimp4.Execute(taskContext);
            csvimp5.Execute(taskContext);
            csvimp6.Execute(taskContext);
            Assert.AreEqual(taskContext.Packages["package0101"], taskContext.Packages["package0102"]);
            Assert.AreEqual(taskContext.Packages["package0101"], taskContext.Packages["package0103"]);
            Assert.AreEqual(taskContext.Packages["package0101"], taskContext.Packages["package0104"]);
            Assert.AreEqual(taskContext.Packages["package0101"], taskContext.Packages["package0105"]);
            //Assert.AreEqual(taskContext.Packages["package0101"], taskContext.Packages["package0106"]);
        }
    }
}
