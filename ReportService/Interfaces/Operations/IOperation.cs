using System.Threading.Tasks;
using ReportService.Interfaces.ReportTask;
using ReportService.Operations;

namespace ReportService.Interfaces.Operations
{
    public interface IOperation
    {
        CommonOperationProperties Properties { get; set; }
        void Execute(IRTaskRunContext taskContext);
        Task ExecuteAsync(IRTaskRunContext taskContext);
    }
}