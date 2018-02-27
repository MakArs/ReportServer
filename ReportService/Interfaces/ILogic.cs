
namespace ReportService.Interfaces
{
    public interface ILogic
    {
        void Execute();
        void ForceExecute(int[] aTaskIDs);
        void Stop();
    }
}
