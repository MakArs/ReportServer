using Dapper;
using ReportService.Interfaces.ReportTask;
using System.Data.Common;
using System.Threading.Tasks;
using Ardalis.GuardClauses;

namespace ReportService.Operations.DataExporters.Dependencies
{
    public class B2BDBStructureChecker : DbStructureChecker
    {
        private string DbStructureCheckQuery => $@"
                IF OBJECT_ID('{ExportTableName}') IS NOT NULL
                IF EXISTS( SELECT * FROM {ExportTableName} 
                            WHERE id = @taskId)
				            AND OBJECT_ID('{ExportInstanceTableName}') IS NOT NULL
                AND EXISTS( SELECT 1 FROM sys.columns 
				            WHERE Name = 'Id' AND Object_ID = Object_ID('{ExportInstanceTableName}'))
				            AND EXISTS(SELECT 1 FROM sys.columns 
				                        WHERE Name = 'Created'
				                        AND Object_ID = Object_ID('{ExportInstanceTableName}')) 
				            AND EXISTS(SELECT 1 FROM sys.columns 
				                        WHERE Name = 'ReportID'
				                        AND Object_ID = Object_ID('{ExportInstanceTableName}')) 
				            AND EXISTS(SELECT 1 FROM sys.columns 
				                        WHERE Name = 'DataPackage'
				                        AND Object_ID = Object_ID('{ExportInstanceTableName}'))  
                SELECT 1
                ELSE SELECT 0";

        public override async Task<bool> CheckIfDbStructureExists(DbConnection connection, IReportTaskRunContext taskContext)
        {
            Guard.Against.Null(taskContext, nameof(taskContext));

            int result = await connection.QueryFirstOrDefaultAsync<int>(new CommandDefinition(DbStructureCheckQuery, new { taskId = taskContext.TaskId }, 
                commandTimeout: DbTimeOut, cancellationToken: taskContext.CancelSource.Token));

            return result == 1;
        }
    }
}
