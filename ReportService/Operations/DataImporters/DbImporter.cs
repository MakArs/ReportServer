using System;
using System.Data.SqlClient;
using System.Threading.Tasks;
using AutoMapper;
using Dapper;
using ReportService.Entities;
using ReportService.Interfaces.Protobuf;
using ReportService.Interfaces.ReportTask;
using ReportService.Operations.DataImporters.Configurations;

namespace ReportService.Operations.DataImporters
{
    public class DbImporter : BaseDbImporter
    {

        public DbImporter(IMapper mapper, DbImporterConfig config,
          IPackageBuilder builder, ThreadSafeRandom rnd) : 
            base(mapper, config,builder, rnd)
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
                    await using var connection = new SqlConnection(ConnectionString);

                    await using var reader =
                        await connection.ExecuteReaderAsync(new CommandDefinition(
                            $"{actualQuery}", parValues, commandTimeout:TimeOut));
                    FillPackage(reader, taskContext);

                    break;
                }

                catch (Exception e)
                {
                    if (!(e is SqlException se))
                        throw e;

                    if (i >= TriesCount - 1)
                        throw se;

                    await Task.Delay(rnd.Next(1000, 60000), token);
                }
            }
        }
    }
}