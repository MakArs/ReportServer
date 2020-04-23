using System.Data.Common;
using System.Data.SqlClient;
using System.Threading.Tasks;
using AutoMapper;
using Dapper;
using ReportService.Interfaces.Core;
using ReportService.Interfaces.ReportTask;
using ReportService.Operations.DataExporters.Configurations;

namespace ReportService.Operations.DataExporters
{
    public class B2BExporter : BaseB2BExporter
    {
        protected override string InsertQuery =>
            $@"INSERT INTO {ExportInstanceTableName}
                (""ReportID"", ""Created"", ""DataPackage"")
                VALUES(@ReportID, @Created, @DataPackage)";

        protected override string DbStructureCheckQuery => $@"
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

        public B2BExporter(IMapper mapper, IArchiver archiver,
            B2BExporterConfig config) : base(mapper, archiver, config)
        {}

        protected override async Task<bool> CheckIfDbStructureExists(DbConnection connection, IReportTaskRunContext taskContext)
        {
           var result= await connection.QueryFirstOrDefaultAsync<int>(new CommandDefinition(DbStructureCheckQuery,
                    new { taskId = taskContext.TaskId }, commandTimeout: DbTimeOut,
                    cancellationToken: taskContext.CancelSource.Token));

            return result == 1;
        }

        public override async Task ExecuteAsync(IReportTaskRunContext taskContext)
        {
            await using var connection = new SqlConnection(ConnectionString);

            await ExportPackage(taskContext, connection);
        }
    }
}