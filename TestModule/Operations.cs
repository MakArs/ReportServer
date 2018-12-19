using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
        [Test]
        public void CsvImporterTest()
        {
            HostConfiguration HostConfigs = new HostConfiguration()
            {
                UrlReservations = new UrlReservations { CreateAutomatically = true },
                RewriteLocalhost = true
            };

            var nancyHost = new NancyHost(
                new Uri("http://localhost:12345"),
                new Bootstrapper(),
                HostConfigs);

            var autofac = Bootstrapper.Global.Resolve<ILifetimeScope>();

            var csvimp= autofac.ResolveNamed<IOperation>("CommonCsvImporter",
                new NamedParameter("config", new CsvImporterConfig
                {
                    FilePath = @"",
                    PackageName = "package0101"
                }));

            var taskContext = autofac.Resolve<IRTaskRunContext>();

            csvimp.Execute(taskContext);
        }
    }
}
