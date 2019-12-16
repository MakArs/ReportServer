using System.Threading.Tasks;

namespace ReportService.Interfaces.ReportTask
{
    public interface ITaskWorker
    {
        void RunTask(IReportTaskRunContext taskContext);

        Task<string> RunTaskAndGetLastViewAsync(IReportTaskRunContext taskContext);

        Task RunTaskAndSendLastViewAsync(IReportTaskRunContext taskContext,
            string mailAddress);
    }
}