using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ReportService.Entities;
using ReportService.Entities.Dto;
using ReportService.Extensions;

namespace ReportService.Interfaces.Core
{
    public interface ILogic
    {
        Dictionary<string, Type> RegisteredExporters { get; set; }
        Dictionary<string, Type> RegisteredImporters { get; set; }

        void CheckScheduleAndExecute();
        void Start();
        string SendDefault(int taskId, string mail);
        string ForceExecute(int taskId);

        string GetAllOperTemplatesJson();
        string GetAllRecepientGroupsJson();
        string GetAllTelegramChannelsJson();
        string GetAllSchedulesJson();
        string GetAllOperationsJson();
        //string GetAllTasksJson();
        string GetEntitiesCountJson();

        int CreateOperationTemplate(DtoOperTemplate operTemplate);
        void UpdateOperationTemplate(DtoOperTemplate operTemplate);
        void DeleteOperationTemplate(int id);

        int CreateRecepientGroup(DtoRecepientGroup group);
        void UpdateRecepientGroup(DtoRecepientGroup group);
        void DeleteRecepientGroup(int id);
        RecipientAddresses GetRecepientAddressesByGroupId(int groupId);

        int CreateTelegramChannel(DtoTelegramChannel channel);
        void UpdateTelegramChannel(DtoTelegramChannel channel);
        DtoTelegramChannel GetTelegramChatIdByChannelId(int id);

        int CreateSchedule(DtoSchedule schedule);
        void UpdateSchedule(DtoSchedule schedule);
        void DeleteSchedule(int id);

        //long CreateTask(ApiTask task); //todo: kick nancy
        //void UpdateTask(ApiTask task);
        void DeleteTask(long taskId);
        Task<string> GetTasksList_HtmlPageAsync();
        Task<string> GetTasksInWorkList_HtmlPageAsync();
        string GetWorkingTasksByIdJson(int id);
        Task<string> GetCurrentViewByTaskIdAsync(int id);

        void DeleteTaskInstanceById(long id);
        string GetAllTaskInstancesByTaskIdJson(int taskId);
        Task<string> GetFullInstanceList_HtmlPageAsync(long taskId);

        void DeleteOperInstanceById(long operInstanceId);
        List<DtoOperInstance> GetOperInstancesByTaskInstanceId(long id);
        DtoOperInstance GetFullOperInstanceById(long id);

        //todo: int CreateEntity<T>(T entity) where T : IDtoEntity;
        //todo: void UpdateEntity<T>(T entity) where T : IDtoEntity;
        //todo: void DeleteEntity<T>(int entityId) or one method for each entity ?
        //todo:replace create&update methods with createorupdate?

        string GetAllRegisteredImportersJson();
        string GetAllRegisteredExportersJson();

        string GetAllB2BExportersJson(string keyParameter);
        //int CreateTaskByTemplate(ApiTask newTask); //todo: kick nancy
        Task<bool> StopTaskByInstanceIdAsync(long taskInstanceId);
    }
}