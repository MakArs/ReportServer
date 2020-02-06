using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Gerakul.FastSql.Common;
using Gerakul.FastSql.SqlServer;
using ReportService.Entities;
using ReportService.Interfaces.Operations;
using ReportService.Interfaces.Protobuf;
using ReportService.Interfaces.ReportTask;
using ReportService.Operations.DataImporters.Configurations;

namespace ReportService.Operations.DataImporters
{
    public class DbImporter : IOperation
    {
        public CommonOperationProperties Properties { get; set; } = new CommonOperationProperties();
        public bool SendVoidPackageError;

        private readonly IPackageBuilder packageBuilder;
        private readonly ThreadSafeRandom rnd;
        private const int TriesCount = 3;

        public string ConnectionString;
        public string Query;
        public int TimeOut;
        public List<string> DataSetNames;
        public string GroupNumbers;

        public DbImporter(IMapper mapper, DbImporterConfig config,
            IPackageBuilder builder, ThreadSafeRandom rnd)
        {
            mapper.Map(config, this);
            mapper.Map(config, Properties);
            Properties.NeedSavePackage = true;
            packageBuilder = builder;
            this.rnd = rnd;
        }

        public async Task ExecuteAsync(IReportTaskRunContext taskContext)
        {
            var parValues = new List<object>();
            var actualQuery = taskContext.SetQueryParameters(parValues, Query);

            for (int i = 0; i < TriesCount; i++)
            {
                try
                {
                    var sqlContext = SqlContextProvider.DefaultInstance
                        .CreateContext(ConnectionString);

                    if (parValues.Count > 0)
                        await sqlContext
                            .CreateSimple(new QueryOptions(TimeOut), $"{actualQuery}",
                                parValues.ToArray())
                            .UseReaderAsync(taskContext.CancelSource.Token, reader =>
                            {
                                FillPackage(reader, taskContext);

                                return Task.CompletedTask;
                            });

                    else
                        await sqlContext
                            .CreateSimple(new QueryOptions(TimeOut), $"{actualQuery}")
                            .UseReaderAsync(taskContext.CancelSource.Token, reader =>
                            {
                                FillPackage(reader, taskContext);

                                return Task.CompletedTask;
                            });
                    break;
                }

                catch (Exception e)
                {
                    if (!(e is SqlException se))
                        throw e; //todo:check

                    if (i >= TriesCount - 1)
                        throw se;

                    await Task.Delay(rnd.Next(1000, 60000), taskContext.CancelSource.Token);
                }
            }
        }

        private void FillPackage(DbDataReader reader, IReportTaskRunContext taskContext)
        {
            var pack = packageBuilder.GetPackage(reader, GroupNumbers);

            if (SendVoidPackageError && !pack.DataSets.Any())
                throw new InvalidDataException("No datasets obtaned during import");

            for (int i = 0; i < DataSetNames.Count; i++)
            {
                if (pack.DataSets.ElementAtOrDefault(i) != null)
                    pack.DataSets[i].Name = DataSetNames[i];
            }

            taskContext.Packages[Properties.PackageName] = pack;
        }
    }
}