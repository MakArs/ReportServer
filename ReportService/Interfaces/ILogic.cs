
using ReportService.Implementations;
using ReportService.Nancy;

namespace ReportService.Interfaces
{
    public interface ILogic
    {
        string ForceExecute(int taskId, string mail);
        string GetTaskList_HtmlPage();
        string GetInstanceList_HtmlPage(int taskId);
        void CreateBase(string aconnstr);

        void Start();
        void Stop();

        void UpdateTask(ApiTask task);
        void DeleteTask(int taskId);
        int CreateTask(ApiTask task);
    }
}
