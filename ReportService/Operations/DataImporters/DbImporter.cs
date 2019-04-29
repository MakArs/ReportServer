using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using Gerakul.FastSql.Common;
using Gerakul.FastSql.SqlServer;
using ReportService.Interfaces.Operations;
using ReportService.Interfaces.Protobuf;
using ReportService.Interfaces.ReportTask;
using ReportService.Operations.DataImporters.Configurations;

namespace ReportService.Operations.DataImporters
{
    public class DbImporter : IOperation
    {
        public CommonOperationProperties Properties { get; set; } = new CommonOperationProperties();

        private readonly IPackageBuilder packageBuilder;

        public string ConnectionString;
        public string Query;
        public int TimeOut;
        public List<string> DataSetNames;

        public DbImporter(IMapper mapper, DbImporterConfig config,
            IPackageBuilder builder)
        {
            mapper.Map(config, this);
            mapper.Map(config, Properties);
            Properties.NeedSavePackage = true;
            packageBuilder = builder;
        }

        public void Execute(IReportTaskRunContext taskContext)
        {
            var sqlContext = SqlContextProvider.DefaultInstance
                .CreateContext(ConnectionString);

            var parValues = new List<object>();
            var actualQuery = taskContext.SetQueryParameters(parValues, Query);

            sqlContext.UsingConnection(connectionContext =>
            {
                if (parValues.Count > 0)
                    connectionContext
                        .CreateSimple(new QueryOptions(TimeOut), $"{actualQuery}",
                            parValues.ToArray())
                        .UseReader(reader =>
                        {
                            var pack = packageBuilder.GetPackage(reader);
                            for (int i = 0; i < DataSetNames.Count; i++)
                                pack.DataSets[i].Name = DataSetNames[i];
                            taskContext.Packages[Properties.PackageName] = pack;
                        });

                else
                    connectionContext
                        .CreateSimple(new QueryOptions(TimeOut), $"{actualQuery}")
                        .UseReader(reader =>
                        {
                            var pack = packageBuilder.GetPackage(reader);
                            for (int i = 0; i < DataSetNames.Count; i++)
                                pack.DataSets[i].Name = DataSetNames[i];
                            taskContext.Packages[Properties.PackageName] = pack;
                        });
            });
        }

        public async Task ExecuteAsync(IReportTaskRunContext taskContext)
        {
            var sqlContext = SqlContextProvider.DefaultInstance
                .CreateContext(ConnectionString);

            var parValues = new List<object>();
            var actualQuery = taskContext.SetQueryParameters(parValues, Query);

            if (parValues.Count > 0)
                await sqlContext
                    .CreateSimple(new QueryOptions(TimeOut), $"{actualQuery}",
                        parValues.ToArray())
                    .UseReaderAsync(taskContext.CancelSource.Token, reader =>
                    {
                        var pack = packageBuilder.GetPackage(reader);
                        for (int i = 0; i < DataSetNames.Count; i++)
                            pack.DataSets[i].Name = DataSetNames[i];
                        taskContext.Packages[Properties.PackageName] = pack;
                        return Task.CompletedTask;
                    });

            else
                await sqlContext
                    .CreateSimple(new QueryOptions(TimeOut), $"{actualQuery}")
                    .UseReaderAsync(taskContext.CancelSource.Token, reader =>
                    {
                        var pack = packageBuilder.GetPackage(reader);
                        for (int i = 0; i < DataSetNames.Count; i++)
                            pack.DataSets[i].Name = DataSetNames[i];
                        taskContext.Packages[Properties.PackageName] = pack;
                        return Task.CompletedTask;
                    });
        }
    }
}