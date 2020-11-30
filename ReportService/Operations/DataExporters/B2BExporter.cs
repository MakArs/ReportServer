using System.Data.SqlClient;
using System.Threading.Tasks;
using AutoMapper;
using ReportService.Interfaces.Core;
using ReportService.Interfaces.ReportTask;
using ReportService.Operations.DataExporters.Configurations;
using ReportService.Operations.DataExporters.Dependencies;

namespace ReportService.Operations.DataExporters
{

    public class B2BExporter : BaseB2BExporter
    {
        protected override string InsertQuery =>
            $@"INSERT INTO {ExportInstanceTableName}
                (""ReportID"", ""Created"", ""DataPackage"")
                VALUES(@ReportID, @Created, @DataPackage)";

        public B2BExporter(IMapper mapper, IArchiver archiver,
            B2BExporterConfig config, B2BDbStructureChecker dbStructureChecker) : base(mapper, archiver, config, dbStructureChecker)
        {
        }

        public override async Task ExecuteAsync(IReportTaskRunContext taskContext)
        {
            await using var connection = new SqlConnection(ConnectionString);

            await ExportPackage(taskContext, connection);
        }
    }
}