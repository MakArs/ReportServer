using System.Threading.Tasks;

namespace ReportService.Interfaces.ReportTask
{
    public interface ITaskWorker
    {
        void RunOperations(IRTaskRunContext taskContext);

        Task<string> RunOperationsAndGetLastViewAsync(IRTaskRunContext taskContext);

        Task RunOperationsAndSendLastViewAsync(IRTaskRunContext taskContext,
            string mailAddress);
    }
}