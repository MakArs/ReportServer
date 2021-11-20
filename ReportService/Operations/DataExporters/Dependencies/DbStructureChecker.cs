using System.Data.Common;
using System.Threading.Tasks;
using Ardalis.GuardClauses;
using ReportService.Interfaces.Core;
using ReportService.Interfaces.ReportTask;
using ReportService.Operations.DataExporters.Configurations;

namespace ReportService.Operations.DataExporters.Dependencies
{
    public abstract class DbStructureChecker : IDBStructureChecker
    {
        public virtual string ExportTableName { get; set; }
        public virtual string ExportInstanceTableName { get; set; }
        public int DbTimeOut { get; set; }

        public abstract Task<bool> CheckIfDbStructureExists(DbConnection connection, IReportTaskRunContext taskContext);

        public void Initialize(B2BExporterConfig config)
        {
            Guard.Against.Null(config, nameof(config));

            ExportTableName = config.ExportTableName;
            ExportInstanceTableName = config.ExportInstanceTableName;
        }
    }
}
