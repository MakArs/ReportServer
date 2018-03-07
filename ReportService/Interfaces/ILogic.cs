
namespace ReportService.Interfaces
{
    public interface ILogic
    {
        string ForceExecute(int aTaskID, string mail);
        string GetTaskView();
        string GetInstancesView(int ataskID);
        void CreateBase(string aconnstr);
        void Start();
        void Stop();
    }
}
