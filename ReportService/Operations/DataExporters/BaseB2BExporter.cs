using AutoMapper;
using Dapper;
using ReportService.Entities;
using ReportService.Interfaces.Core;
using ReportService.Interfaces.Operations;
using ReportService.Interfaces.ReportTask;
using ReportService.Operations.DataExporters.Configurations;
using System;
using System.Data.Common;
using Google.Protobuf;
using System.Threading.Tasks;
using System.IO;

namespace ReportService.Operations.DataExporters
{
    public abstract class BaseB2BExporter : IOperation
    {
        public bool CreateDataFolder { get; set; }
        public CommonOperationProperties Properties { get; set; } = new CommonOperationProperties();
        public bool RunIfVoidPackage { get; set; }

        protected readonly IArchiver archiver;

        public string ConnectionString;
        public string ExportTableName;
        public string ExportInstanceTableName;
        public int DbTimeOut;

        protected abstract string InsertQuery { get; }
        protected abstract string DbStructureCheckQuery { get; }

        public BaseB2BExporter(IMapper mapper, IArchiver archiver,
            B2BExporterConfig config)
        {
            this.archiver = archiver;
            mapper.Map(config, this);
            mapper.Map(config, Properties);
        }

        protected abstract Task<bool> CheckIfDbStructureExists(DbConnection connection, IReportTaskRunContext taskContext);

        protected async Task ExportPackage(IReportTaskRunContext taskContext, DbConnection connection)
        {
            var package = taskContext.Packages[Properties.PackageName];

            if (!RunIfVoidPackage && package.DataSets.Count == 0)
                return;

            var token = taskContext.CancelSource.Token;

            var dbStructureExists = await CheckIfDbStructureExists(connection, taskContext);

            if (!dbStructureExists)
            {
                var msg = $"The export database structure doesn't contain the data required for export. Required ExportTableName: {ExportTableName}, ExportInstanceTableName: {ExportInstanceTableName}.";
                throw new Exception(msg);
            }

            byte[] archivedPackage;

            await using (var stream = new MemoryStream())
            {
                package.WriteTo(stream);
                archivedPackage = archiver.CompressStream(stream);
            }

            var newInstance = new
            {
                ReportID = taskContext.TaskId,
                Created = DateTime.Now,
                DataPackage = archivedPackage
            };

            await connection.ExecuteAsync(new CommandDefinition(InsertQuery,
                newInstance, commandTimeout: DbTimeOut,
                cancellationToken: token));
        }

        public abstract Task ExecuteAsync(IReportTaskRunContext taskContext);
    }
}
