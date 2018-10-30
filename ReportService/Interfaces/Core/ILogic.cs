using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ReportService.Extensions;
using ReportService.Nancy;

namespace ReportService.Interfaces.Core
{
    public interface ILogic
    {
        Dictionary<string, Type> RegisteredExporters { get; set; }
        Dictionary<string, Type> RegisteredImporters { get; set; }

        void Start();
        void Stop();
        string SendDefault(int taskId, string mail);
        string ForceExecute(int taskId);

        string GetAllOperTemplatesJson();
        string GetAllRecepientGroupsJson();
        string GetAllTelegramChannelsJson();
        string GetAllSchedulesJson();
        string GetAllOperationsJson();
        string GetAllTasksJson();
        string GetInWorkEntitiesJson();

        int CreateOperationTemplate(DtoOperTemplate operTemplate);
        void UpdateOperationTemplate(DtoOperTemplate operTemplate);
        void DeleteOperationTemplate(int id);

        int CreateRecepientGroup(DtoRecepientGroup group);
        void UpdateRecepientGroup(DtoRecepientGroup group);
        void DeleteRecepientGroup(int id);
        RecepientAddresses GetRecepientAddressesByGroupId(int groupId);

        int CreateTelegramChannel(DtoTelegramChannel channel);
        void UpdateTelegramChannel(DtoTelegramChannel channel);
        DtoTelegramChannel GetTelegramChatIdByChannelId(int id);

        int CreateSchedule(DtoSchedule schedule);
        void UpdateSchedule(DtoSchedule schedule);
        void DeleteSchedule(int id);

        int CreateTask(ApiTask task);
        void UpdateTask(ApiTask task);
        void DeleteTask(int taskId);
        Task<string> GetTaskList_HtmlPage();
        Task<string> GetCurrentViewByTaskId(int id);

        void DeleteTaskInstanceById(int id);
        string GetAllTaskInstancesJson();
        string GetAllTaskInstancesByTaskIdJson(int taskId);
        Task<string> GetFullInstanceList_HtmlPage(int taskId);

        void DeleteOperInstanceById(int operInstanceId);
        string GetOperInstancesByTaskInstanceIdJson(int id);
        string GetFullOperInstanceByIdJson(int id);

        //todo: int CreateEntity<T>(T entity) where T : IDtoEntity;
        //todo: void UpdateEntity<T>(T entity) where T : IDtoEntity;
        //todo: void DeleteEntity<T>(int entityId) or one method for each entity ??
        //todo:replace create&update methods with createorupdate?

        string GetAllRegisteredImporters();
        string GetAllRegisteredExporters();
    }
}