
using ReportService.Implementations;

namespace ReportService.Interfaces
{
    public interface ILogic
    {
        string ForceExecute(int taskId, string mail);
        string GetTaskView();
        string GetInstancesView(int ataskID);
        void CreateBase(string aconnstr);

        void Start();
        void Stop();

        void UpdateTask(int taskId, RTask task);
        void DeleteTask(int taskId);
        int CreateTask(RTask task);
    }
}
