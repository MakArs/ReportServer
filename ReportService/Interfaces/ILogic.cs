
namespace ReportService.Interfaces
{
    public interface ILogic
    {
        string ForceExecute(int aTaskIDs,string mail);
        void Start();
        void Stop();
    }
}
