using ReportService.Interfaces.Core;
using ReportService.Interfaces.ReportTask;
using System.Data.Common;
using System.Threading.Tasks;

namespace ReportService.Operations.DataExporters
{
    public abstract class DBStructureChecker : IDBStructureChecker
    {

        #region IDBStructureChecker

        public string ExportTableName { get; set; }
        public string ExportInstanceTableName { get; set; }
        public int DbTimeOut { get; set; }

        public abstract Task<bool> CheckIfDbStructureExists(DbConnection connection, IReportTaskRunContext taskContext);
        #endregion
    }
}