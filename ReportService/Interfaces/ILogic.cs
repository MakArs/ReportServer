
namespace ReportService.Interfaces
{
    public interface ILogic
    {
        string ForceExecute(int aTaskIDs);
        void Start();
        void Stop();
    }
}
