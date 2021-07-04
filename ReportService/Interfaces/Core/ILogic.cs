using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ReportService.Entities;
using ReportService.Entities.Dto;
using ReportService.Interfaces.ReportTask;

namespace ReportService.Interfaces.Core
{
    public interface ILogic
    {
        Dictionary<string, Type> RegisteredExporters { get; set; }
        Dictionary<string, Type> RegisteredImporters { get; set; }

        void CheckScheduleAndExecute();
        void Start();
        string SendDefault(int taskId, string mail);
        string ForceExecute(long taskId);

        string GetAllOperTemplatesJson();
        string GetAllRecepientGroupsJson();
        string GetAllTelegramChannelsJson();
        string GetAllSchedulesJson();
        string GetAllOperationsJson();
        List<IReportTask> GetAllTasksJson();
        string GetEntitiesCountJson();

        long CreateOperationTemplate(DtoOperTemplate operTemplate);
        void UpdateOperationTemplate(DtoOperTemplate operTemplate);
        void DeleteOperationTemplate(int id);

        long CreateRecepientGroup(DtoRecepientGroup group);
        void UpdateRecepientGroup(DtoRecepientGroup group);
        void DeleteRecepientGroup(int id);
        RecipientAddresses GetRecepientAddressesByGroupId(int groupId);

        long CreateTelegramChannel(DtoTelegramChannel channel);
        void UpdateTelegramChannel(DtoTelegramChannel channel);
        DtoTelegramChannel GetTelegramChatIdByChannelId(int id);

        long CreateSchedule(DtoSchedule schedule);
        void UpdateSchedule(DtoSchedule schedule);
        void DeleteSchedule(int id);

        long CreateTask(DtoTask task, DtoOperation[] bindedOpers);
        void UpdateTask(DtoTask task, DtoOperation[] bindedOpers);
        void DeleteTask(long taskId);
        Task<string> GetTasksList_HtmlPageAsync();
        Task<string> GetTasksInWorkList_HtmlPageAsync();
        string GetWorkingTaskInstancesJson(long taskId);
        Task<string> GetCurrentViewAsync(long taskId);

        void DeleteTaskInstanceById(long taskInstanceId);
        Task<string> GetAllTaskInstancesJson(long taskId);
        Task<string> GetFullInstanceList_HtmlPageAsync(long taskId);

        List<DtoOperInstance> GetOperInstancesByTaskInstanceId(long id);
        DtoOperInstance GetFullOperInstanceById(long id);

        //todo: int CreateEntity<T>(T entity) where T : IDtoEntity;
        //todo: void UpdateEntity<T>(T entity) where T : IDtoEntity;
        //todo: void DeleteEntity<T>(int entityId) or one method for each entity ?
        //todo:replace create&update methods with createorupdate?

        string GetAllRegisteredImportersJson();
        string GetAllRegisteredExportersJson();

        string GetAllB2BExportersJson(string keyParameter);

        //int CreateTaskByTemplate(ApiTask newTask); 
        Task<bool> StopTaskInstanceAsync(long taskInstanceId);
    }
}