﻿using ReportService.Nancy;

namespace ReportService.Interfaces
{
    public interface ILogic
    {
        void   Start();
        void   Stop();
        string ForceExecute(int taskId, string mail);

        string GetTaskList_HtmlPage();
        string GetFullInstanceList_HtmlPage(int taskId);

        string GetAllTasksJson();
        string GetFullTaskByIdJson(int id);
        void   DeleteTask(int taskId);
        int    CreateTask(ApiTask task);
        void   UpdateTask(ApiTask task);

        string GetAllInstancesJson();
        string GetAllInstancesByTaskIdJson(int taskId);
        string GetFullInstanceByIdJson(int id);
        void   DeleteInstance(int instanceId);

        int  CreateReport(DtoReport report);
        void UpdateReport(DtoReport report);

        string GetAllSchedulesJson();
        string GetAllRecepientGroupsJson();
        string GetAllReportsJson();

        string GetCurrentViewByTaskId(int id);

        string GetAllCustomDataExecutors();
        string GetAllCustomViewExecutors();
    }
}
