
namespace ReportService.Interfaces
{
    public interface ILogic
    {
        string ForceExecute(int aTaskID, string mail);
        void Start();
        void Stop();
    }
}
