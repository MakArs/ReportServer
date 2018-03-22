
using ReportService.Implementations;

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
        void UpdateTask(int ataskID, RTask atask);
        void DeleteTask(int ataskID);
        int CreateTask(RTask atask);
    }
}
