using System;
using System.Data.SqlClient;
using System.IO;
using System.Threading.Tasks;
using AutoMapper;
using Dapper;
using Google.Protobuf;
using ReportService.Entities;
using ReportService.Interfaces.Core;
using ReportService.Interfaces.Operations;
using ReportService.Interfaces.ReportTask;
using ReportService.Operations.DataExporters.Configurations;

namespace ReportService.Operations.DataExporters
{
    public class B2BExporter : IOperation
    {
        public CommonOperationProperties Properties { get; set; } = new CommonOperationProperties();
        public bool RunIfVoidPackage { get; set; }

        private readonly IArchiver archiver;

        private string InsertQuery =>
            $@"insert into {ExportInstanceTableName} 
                ([ReportID], [Created], [DataPackage])
                VALUES(@ReportID, @Created, @DataPackage)";

        private string DbStructureCheckQuery => $@"
                IF OBJECT_ID('{ExportTableName}') IS NOT NULL
                IF EXISTS(SELECT * FROM {ExportTableName} WHERE id = @taskId)
				AND OBJECT_ID('{ExportInstanceTableName}') IS NOT NULL
                AND  EXISTS(SELECT 1 FROM sys.columns 
				  WHERE Name = 'Id'
				  AND Object_ID = Object_ID('{ExportInstanceTableName}'))
				  AND  EXISTS(SELECT 1 FROM sys.columns 
				  WHERE Name = 'Created'
				  AND Object_ID = Object_ID('{ExportInstanceTableName}')) 
				  AND  EXISTS(SELECT 1 FROM sys.columns 
				  WHERE Name = 'ReportID'
				  AND Object_ID = Object_ID('{ExportInstanceTableName}')) 
				  AND  EXISTS(SELECT 1 FROM sys.columns 
				  WHERE Name = 'DataPackage'
				  AND Object_ID = Object_ID('{ExportInstanceTableName}'))  
                SELECT 1
                ELSE SELECT 0";

        public string ConnectionString;
        public string ExportTableName;
        public string ExportInstanceTableName;
        public int DbTimeOut;

        public B2BExporter(IMapper mapper, IArchiver archiver,
            B2BExporterConfig config)
        {
            this.archiver = archiver;
            mapper.Map(config, this);
            mapper.Map(config, Properties);
        }

        public async Task ExecuteAsync(IReportTaskRunContext taskContext)
        {
            var package = taskContext.Packages[Properties.PackageName];

            if (!RunIfVoidPackage && package.DataSets.Count == 0)
                return;

            var token = taskContext.CancelSource.Token;

            await using var connection = new SqlConnection(ConnectionString);

            if (await connection.QueryFirstOrDefaultAsync<int>(new CommandDefinition(DbStructureCheckQuery, 
                    new { taskId = taskContext.TaskId }, commandTimeout: DbTimeOut,
                    cancellationToken: token)) != 1)
            {
                var msg = "The export database structure doesn't contain the data required for export";
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
    }
}