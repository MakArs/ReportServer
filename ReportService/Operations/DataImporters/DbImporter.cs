using AutoMapper;
using Gerakul.FastSql.Common;
using Gerakul.FastSql.SqlServer;
using ReportService.Interfaces.Core;
using ReportService.Interfaces.Protobuf;
using ReportService.Interfaces.ReportTask;

namespace ReportService.Operations.DataImporters
{
    public class DbImporter : IDataImporter
    {
        private readonly IPackageBuilder packageBuilder;

        public int Id { get; set; }
        public bool IsDefault { get; set; }
        public int Number { get; set; }
        public string Name { get; set; }
        public string PackageName { get; set; }
        public string ConnectionString;
        public string Query;
        public int TimeOut;

        public DbImporter(IMapper mapper, DbImporterConfig config, IPackageBuilder builder)
        {
            mapper.Map(config, this);
            packageBuilder = builder;
        }

        public void Execute(IRTaskRunContext taskContext)
        {
            var sqlContext = SqlContextProvider.DefaultInstance
                .CreateContext(ConnectionString);

            sqlContext.UsingConnection(connectionContext =>
            {
                var opt = new QueryOptions(TimeOut);
                connectionContext
                    .CreateSimple(opt, $"{Query}")
                    .UseReader(reader =>
                    {
                        var pack = packageBuilder.GetPackage(reader);
                        var gsd = pack.DataSets.Count;
                        taskContext.Packages[PackageName] = pack;
                    });
            });

        }
    }
}