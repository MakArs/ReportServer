using System.Threading.Tasks;
using ReportService.Entities;
using ReportService.Interfaces.ReportTask;

namespace ReportService.Interfaces.Operations
{
    public interface IOperation
    {
        CommonOperationProperties Properties { get; set; }
        bool CreateDataFolder { get; set; }
        Task ExecuteAsync(IReportTaskRunContext taskContext);
    }
}