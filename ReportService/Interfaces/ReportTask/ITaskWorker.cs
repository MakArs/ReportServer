using System.Collections.Generic;
using System.Threading.Tasks;
using ReportService.Interfaces.Core;

namespace ReportService.Interfaces.ReportTask
{
    public interface ITaskWorker
    {
        void RunOperations(List<IOperation> opers, IRTaskRunContext taskContext);

        Task<string> RunOperationsAndGetLastView(
            List<IOperation> opers, IRTaskRunContext taskContext);

        void RunOperationsAndSendLastView(List<IOperation> opers,
                                          IRTaskRunContext taskContext,
                                          string mailAddress);
    }
}
