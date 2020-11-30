using ReportService.Interfaces.ReportTask;
using System.Data.Common;
using System.Threading.Tasks;

namespace ReportService.Interfaces.Core
{
    public interface IDBStructureChecker
    {
        Task<bool> CheckIfDbStructureExists(DbConnection connection, IReportTaskRunContext taskContext);
    }
}