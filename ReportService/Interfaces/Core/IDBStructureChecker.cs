using ReportService.Interfaces.ReportTask;
using ReportService.Operations.DataExporters.Configurations;
using System.Data.Common;
using System.Threading.Tasks;

namespace ReportService.Interfaces.Core
{
    public interface IDBStructureChecker
    {
        public void Initialize(B2BExporterConfig config);
        Task<bool> CheckIfDbStructureExists(DbConnection connection, IReportTaskRunContext taskContext);
    }
}