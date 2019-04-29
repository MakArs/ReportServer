using System.Threading.Tasks;

namespace ReportService.Interfaces.ReportTask
{
    public interface ITaskWorker
    {
        void RunOperations(IReportTaskRunContext taskContext);

        Task<string> RunOperationsAndGetLastViewAsync(IReportTaskRunContext taskContext);

        Task RunOperationsAndSendLastViewAsync(IReportTaskRunContext taskContext,
            string mailAddress);
    }
}