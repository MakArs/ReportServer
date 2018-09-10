using System;
using ReportService.Extensions;
using ReportService.Nancy;

namespace ReportService.Interfaces
{
    public interface ILogic
    {
        void Start();
        void Stop();
        string ForceExecute(int taskId, string mail);

        string GetAllOperationsJson();
        string GetAllRecepientGroupsJson();
        string GetAllTelegramChannelsJson();
        string GetAllSchedulesJson();
        string GetAllTaskOpersJson();
        string GetAllTasksJson();

        int CreateOperation(DtoOper oper);
        void UpdateOperation(DtoOper oper);

        int CreateRecepientGroup(DtoRecepientGroup group);
        void UpdateRecepientGroup(DtoRecepientGroup group);
        RecepientAddresses GetRecepientAddressesByGroupId(int groupId);

        int CreateTelegramChannel(DtoTelegramChannel channel);
        void UpdateTelegramChannel(DtoTelegramChannel channel);
        DtoTelegramChannel GetTelegramChatIdByChannelId(int id);

        int CreateSchedule(DtoSchedule schedule);
        void UpdateSchedule(DtoSchedule schedule);

        int CreateTaskOper(DtoTaskOper taskOper);

        int CreateTask(ApiTask task);
        void UpdateTask(ApiTask task);
        void DeleteTask(int taskId);
        string GetTaskList_HtmlPage();
        string GetCurrentViewByTaskId(int id);

        void DeleteTaskInstanceById(int id);
        string GetAllTaskInstancesJson();
        string GetAllTaskInstancesByTaskIdJson(int taskId);
        string GetFullInstanceList_HtmlPage(int taskId);

        void DeleteOperInstanceById(int operInstanceId);
        string GetAllOperInstancesByTaskInstanceIdJson(int taskInstanceId);
        string GetFullOperInstanceByIdJson(int id);

        //todo: int CreateEntity<T>(T entity) where T : IDtoEntity;
        //todo: void UpdateEntity<T>(T entity) where T : IDtoEntity;
        //todo: void DeleteEntity<T>(int entityId) or one method for each entity ??

        string GetAllCustomDataExecutors();
        string GetAllCustomViewExecutors();
    }
}
//todo:replace create&update methods with createorupdate?