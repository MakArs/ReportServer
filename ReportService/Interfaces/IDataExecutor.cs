using ReportService.Core;

namespace ReportService.Interfaces
{
    public interface IDataExecutor
    {
        string Execute(RTask task);
    }
}
