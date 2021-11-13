using System;
using System.Data.SqlClient;
using System.Threading.Tasks;
using AutoMapper;
using ReportService.Entities;
using ReportService.Interfaces.Protobuf;
using ReportService.Interfaces.ReportTask;
using ReportService.Operations.DataImporters.Configurations;
using ReportService.Operations.DataImporters.Helpers;

namespace ReportService.Operations.DataImporters
{
    public class DbPackageDataConsumer : PackageConsumerBase
    {

        public DbPackageDataConsumer(
            IMapper mapper,
            DbImporterConfig config, 
            IPackageBuilder builder, 
            ThreadSafeRandom rnd,
            DbPackageExportScriptCreator scriptCreator,
            SqlCommandInitializer sqlCommandInitializer) 
            : base(mapper, config, builder, rnd, scriptCreator, sqlCommandInitializer)
        {
        }

        protected async override Task ExecuteCommand(IReportTaskRunContext taskContext)
        {
            var token = taskContext.CancelSource.Token;
            for (int i = 0; i < TriesCount; i++)
            {
                using var connection = new SqlConnection(ConnectionString);
                connection.Open();
                using var transaction = connection.BeginTransaction();
                try
                {
                    var resultedExportPackagesCommand = commandInitializer.ResolveCommand();
                    resultedExportPackagesCommand.Connection = connection;
                    resultedExportPackagesCommand.Transaction = transaction;

                    using var reader = await resultedExportPackagesCommand.ExecuteReaderAsync(token);
                    FillPackage(reader, taskContext);

                    break;
                }
                catch (Exception e)
                {
                    if (!(e is SqlException se))
                        throw e;

                    if (i >= TriesCount - 1)
                        throw se;

                    transaction.Rollback();
                    await Task.Delay(rnd.Next(1000, 60000), token);
                }
            }
        }
    }
}
