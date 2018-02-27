
namespace ReportService.Interfaces
{
    public interface ILogic
    {
        void Execute();
        string ForceExecute(int aTaskIDs);
        void Stop();
    }
}
