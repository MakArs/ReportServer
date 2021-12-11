using AutoMapper;
using Npgsql;
using ReportService.Interfaces.Core;
using ReportService.Interfaces.ReportTask;
using ReportService.Operations.DataExporters.Configurations;
using ReportService.Operations.DataExporters.Dependencies;
using System.Threading.Tasks;

namespace ReportService.Operations.DataExporters
{
	public class PostgresB2BExporter : BaseB2BExporter
	{
		protected override string InsertQuery =>
			$@" INSERT INTO ""{ExportInstanceTableName}""
						(""ReportID"", ""Created"", ""DataPackage"")
						VALUES(@ReportID, @Created, @DataPackage);";

	
		public PostgresB2BExporter(IMapper mapper, IArchiver archiver,
			B2BExporterConfig config, PostrgresDBStructureChecker dBStructureChecker) : base(mapper, archiver, config, dBStructureChecker)
		{
		}

		public override async Task ExecuteAsync(IReportTaskRunContext taskContext)
		{
			await using var connection = new NpgsqlConnection(ConnectionString);

			await ExportPackage(taskContext, connection);
		}
	}
}