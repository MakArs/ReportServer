using System;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Autofac;
using Gerakul.FastSql.Common;
using Gerakul.FastSql.SqlServer;
using Nancy.Hosting.Self;
using NUnit.Framework;
using ReportService;
using ReportService.Entities;
using ReportService.Entities.Dto;
using ReportService.Interfaces.Core;
using ReportService.Interfaces.Operations;
using ReportService.Interfaces.ReportTask;
using ReportService.Operations.DataExporters.Configurations;

namespace TestModule
{
    [TestFixture]
    public class OneTimeTasks
    {
        private readonly ILifetimeScope autofac;
        private IArchiver extractor;
        private IArchiver archiver;

        public OneTimeTasks()
        {
            HostConfiguration hostConfigs = new HostConfiguration()
            {
                UrlReservations = new UrlReservations { CreateAutomatically = true },
                RewriteLocalhost = true
            };

            var nancyHost = new NancyHost(
                new Uri("http://localhost:12345"),
                new Bootstrapper(),
                hostConfigs);

            autofac = Bootstrapper.Global.Resolve<ILifetimeScope>();
            extractor = autofac.ResolveNamed<IArchiver>("7Zip");
            archiver = autofac.ResolveNamed<IArchiver>("Zip");
        }

        
        public void BaseFunctions()
        {
            var strs = "test @RepPar";
            var paramName = new Regex(@"\B\@RepPar\w*\b");
            var selections = paramName.Matches(strs);

            var repos = autofac.Resolve<IRepository>();
            var fsaf = repos.GetBaseQueryResult("select getdate()-7 as result");
        }
        
        public void ArchiveChanging()
        {
           var context = SqlContextProvider.DefaultInstance.CreateContext(ConfigurationManager.AppSettings["DBConnStr"]);
        
            var updatedinstances = context.CreateSimple
                ("select id,TaskInstanceId,OperationId,StartTime,Duration,State,DataSet,ErrorMessage " +
                 "from [OperInstance] where DataSet is not null")
                .ExecuteQuery<DtoOperInstance>()
                .ToList();
           
            StringBuilder errs = new StringBuilder();
            foreach (var instance in updatedinstances.Where(inst=>inst.DataSet!=null))
            {
                try
                {
                    instance.DataSet = ChangeArchiveFormat(instance.DataSet);
                }
                catch (Exception ex)
                {
                    errs.AppendLine(ex.Message);
                }
            }

            //foreach (var instance in updatedinstances)
            //    context.Update("OperInstance", instance, "Id");

            // Assert.AreNotEqual(updatedinstances,instances);
            //  var tt = errs.ToString();
        }

        public byte[] ChangeArchiveFormat(byte[] archivedValue)
        {
           
            var value = extractor.ExtractFromByteArchive(archivedValue);
            var newFormatValue = archiver.CompressByteArray(value);
            return newFormatValue;
        }
    }
}
