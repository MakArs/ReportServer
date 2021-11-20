using Dapper;
using ReportService.Interfaces.ReportTask;
using System.Data.Common;
using System.Threading.Tasks;

namespace ReportService.Operations.DataExporters.Dependencies
{
    public class PostrgresDBStructureChecker : DbStructureChecker
    {
        private string DbStructureCheckQuery => $@"SELECT 
		  CASE WHEN 
		  ((SELECT EXISTS(SELECT * 
						FROM information_schema.columns 
						WHERE table_name = '{ExportTableName}'  and column_name='Id'))	
			AND (SELECT EXISTS(SELECT column_name 
						FROM information_schema.columns 
						WHERE table_name='{ExportInstanceTableName}' and column_name='Id'))
			AND (SELECT EXISTS(SELECT column_name 
						FROM information_schema.columns 
						WHERE table_name='{ExportInstanceTableName}' and column_name='Created'))
			AND (SELECT EXISTS(SELECT column_name 
						FROM information_schema.columns 
						WHERE table_name='{ExportInstanceTableName}' and column_name='ReportID'))
			AND (SELECT EXISTS(SELECT column_name 
						FROM information_schema.columns 
						WHERE table_name='{ExportInstanceTableName}' and column_name='DataPackage')))
			THEN 1
			ELSE 0
		  END";
        private string CheckTaskIdRowQuery => $@"SELECT EXISTS(
						SELECT 1 
						FROM ""{ExportTableName}""
						WHERE ""Id"" =@TaskId
						)";

        public override async Task<bool> CheckIfDbStructureExists(DbConnection connection, IReportTaskRunContext taskContext)
        {
            var tablesStructure = await connection.QueryFirstOrDefaultAsync<int>(new CommandDefinition(DbStructureCheckQuery, commandTimeout: DbTimeOut,
                    cancellationToken: taskContext.CancelSource.Token));

            if (tablesStructure == 0)
                return false;

            var result = await connection.QueryFirstOrDefaultAsync<bool>(new CommandDefinition(CheckTaskIdRowQuery,
                     new { taskId = taskContext.TaskId }, commandTimeout: DbTimeOut,
                     cancellationToken: taskContext.CancelSource.Token));

            return result;
        }
    }
}
