using System.Collections.Generic;
using System.Threading.Tasks;
using ReportService.Interfaces.Core;

namespace ReportService.Interfaces.ReportTask
{
    public interface ITaskWorker
    {
        void RunOperations(IRTaskRunContext taskContext);

        Task<string> RunOperationsAndGetLastView(IRTaskRunContext taskContext);

        void RunOperationsAndSendLastView(IRTaskRunContext taskContext,
            string mailAddress);
    }
}