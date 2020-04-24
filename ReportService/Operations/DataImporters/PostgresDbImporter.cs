using AutoMapper;
using Dapper;
using ReportService.Entities;
using ReportService.Interfaces.Protobuf;
using ReportService.Interfaces.ReportTask;
using ReportService.Operations.DataImporters.Configurations;
using System;
using Npgsql;
using System.Threading.Tasks;

namespace ReportService.Operations.DataImporters
{
    public class PostgresDbImporter: BaseDbImporter
    {        
        public PostgresDbImporter(IMapper mapper, DbImporterConfig config,
            IPackageBuilder builder, ThreadSafeRandom rnd) :
            base(mapper, config, builder, rnd)
        { }

        public override async Task ExecuteAsync(IReportTaskRunContext taskContext)
        {
            var parValues = new DynamicParameters();

            var actualQuery = taskContext.SetQueryParameters(parValues, Query);

            var token = taskContext.CancelSource.Token;

            for (int i = 0; i < TriesCount; i++)
            {
                try
                {
                    await using var connection = new NpgsqlConnection(ConnectionString);

                    await using var reader =
                        await connection.ExecuteReaderAsync(new CommandDefinition(
                            $"{actualQuery}", parValues, commandTimeout: TimeOut));
                    FillPackage(reader, taskContext);

                    break;
                }

                catch (Exception e)
                {
                    if (!(e is NpgsqlException se))
                        throw e;

                    if (i >= TriesCount - 1)
                        throw se;

                    await Task.Delay(rnd.Next(1000, 60000), token);
                }
            }
        }
    }
}