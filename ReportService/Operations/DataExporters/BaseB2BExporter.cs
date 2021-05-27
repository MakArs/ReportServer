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
        public CommonOperationProperties Properties { get; set; } = new CommonOperationProperties();

        public bool CreateDataFolder { get; set; }
        public bool RunIfVoidPackage { get; set; }
        public string ExportTableName { get; set; }
        public string ExportInstanceTableName { get; set; }
        public int DbTimeOut { get; set; }
        public string ConnectionString;

        protected readonly IArchiver archiver;
        protected readonly IDBStructureChecker dbStructureChecker;
        protected abstract string InsertQuery { get; }

        public BaseB2BExporter(IMapper mapper
            , IArchiver archiver
            , B2BExporterConfig config
            , IDBStructureChecker dbStructureChecker)
        {
            this.archiver = archiver;
            this.dbStructureChecker = dbStructureChecker;
            mapper.Map(config, this);
            mapper.Map(config, Properties);
            dbStructureChecker.Initialize(config);
        }

        protected async Task ExportPackage(IReportTaskRunContext taskContext, DbConnection connection)
        {
            var package = taskContext.Packages[Properties.PackageName];

            if (!RunIfVoidPackage && package.DataSets.Count == 0)
                return;

            var token = taskContext.CancelSource.Token;

            var dbStructureExists = await dbStructureChecker.CheckIfDbStructureExists(connection,taskContext);

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