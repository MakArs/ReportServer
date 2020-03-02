using AutoMapper;
using Dapper;
using Npgsql;
using ReportService.Interfaces.Core;
using ReportService.Interfaces.ReportTask;
using ReportService.Operations.DataExporters.Configurations;
using System.Data.Common;
using System.Threading.Tasks;

namespace ReportService.Operations.DataExporters
{
	public class PostgresB2BExporter : BaseB2BExporter
	{
		protected override string InsertQuery =>
			$@" INSERT INTO ""{ExportInstanceTableName}""
						(""ReportID"", ""Created"", ""DataPackage"")
						VALUES(@ReportID, @Created, @DataPackage);";

		protected override string DbStructureCheckQuery => $@"SELECT 
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

		public PostgresB2BExporter(IMapper mapper, IArchiver archiver,
			B2BExporterConfig config) : base(mapper, archiver, config)
		{ }

		public override async Task ExecuteAsync(IReportTaskRunContext taskContext)
		{
			await using var connection = new NpgsqlConnection(ConnectionString);

			await ExportPackage(taskContext, connection);
		}

		protected override async Task<bool> CheckIfDbStructureExists(DbConnection connection, IReportTaskRunContext taskContext)
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