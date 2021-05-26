using ReportService.Interfaces.Core;
using ReportService.Interfaces.ReportTask;
using ReportService.Operations.DataExporters.Configurations;
using System.Data.Common;
using System.Threading.Tasks;

namespace ReportService.Operations.DataExporters
{
    public abstract class DBStructureChecker : IDBStructureChecker
    {

        #region IDBStructureChecker

        public virtual string ExportTableName { get; set; }
        public virtual string ExportInstanceTableName { get; set; }
        public int DbTimeOut { get; set; }

        public abstract Task<bool> CheckIfDbStructureExists(DbConnection connection, IReportTaskRunContext taskContext);

        public void Initialize(B2BExporterConfig config)
        {
            ExportTableName = config.ExportTableName;
            ExportInstanceTableName = config.ExportInstanceTableName;
        }
        #endregion
    }
}