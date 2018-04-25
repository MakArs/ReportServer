using ReportService.Nancy;

namespace ReportService.Interfaces
{
    public interface ILogic
    {
        void Start();
        void Stop();
        string ForceExecute(int taskId, string mail);

        string GetTaskList_HtmlPage();
        string GetFullInstanceList_HtmlPage(int taskId);

        string GetAllTasksJson();
        string GetFullTaskByIdJson(int id);
        void DeleteTask(int taskId);
        int CreateTask(ApiFullTask task);
        void UpdateTask(ApiFullTask task);

        string GetAllInstancesJson();
        string GetAllInstancesByTaskIdJson(int taskId);
        string GetFullInstanceByIdJson(int id);
        void DeleteInstance(int instanceId);

        string GetAllSchedulesJson();
        string GetAllRecepientGroupsJson();

        string GetCurrentViewByTaskId(int id);
    }
}
