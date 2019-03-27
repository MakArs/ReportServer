using System;
using System.Collections.Generic;

namespace ReportService.Interfaces.Core
{
    public interface IDtoEntity
    {
    }

    public class DtoOperTemplate : IDtoEntity
    {
        public int Id;
        public string ImplementationType;
        public string Name;
        public string ConfigTemplate;
    }

    public class DtoRecepientGroup : IDtoEntity
    {
        public int Id;
        public string Name;
        public string Addresses;
        public string AddressesBcc;
    }

    public class DtoTelegramChannel : IDtoEntity
    {
        public int Id;
        public string Name;
        public string Description;
        public long ChatId;
        public int Type; //from nuget types enum
    }

    public class DtoSchedule : IDtoEntity
    {
        public int Id;
        public string Name;
        public string Schedule;
    }

    public class DtoTask : IDtoEntity
    {
        public int Id;
        public string Name;
        public string Parameters;
        public int? ScheduleId;
    }

    public class DtoOperation : IDtoEntity
    {
        public int Id;
        public int TaskId;
        public int Number;
        public string Name;
        public string ImplementationType;
        public bool IsDefault;
        public string Config;
        public bool IsDeleted;
    }

    public class DtoTaskInstance : IDtoEntity
    {
        public int Id;
        public int TaskId;
        public DateTime StartTime;
        public int Duration;
        public int State;
    }

    public class DtoOperInstance : IDtoEntity
    {
        public int Id;
        public int TaskInstanceId;
        public int OperationId;
        public DateTime StartTime;
        public int Duration;
        public int State;
        public byte[] DataSet;
        public string ErrorMessage;
    }

    public interface IRepository
    {
        object GetBaseQueryResult(string query);
        List<DtoTaskInstance> GetInstancesByTaskId(int taskId);
        List<DtoOperInstance> GetOperInstancesByTaskInstanceId(int taskInstanceId);
        DtoOperInstance GetFullOperInstanceById(int operInstanceId);

        /// <summary>
        /// Obtains list of generic-type entities from repository.
        /// WARNING: generic type name must be database table name with "Dto" prefix
        /// </summary>
        List<T> GetListEntitiesByDtoType<T>() where T : IDtoEntity, new();

        /// <summary>
        /// Creates generic-type entity in repository.
        /// WARNING: generic type name must be database table name with "Dto" prefix
        /// </summary>
        int CreateEntity<T>(T entity) where T : IDtoEntity;

        int CreateTask(DtoTask task, params DtoOperation[] bindedOpers);

        /// <summary>
        /// Updates generic-type entity in repository.
        /// WARNING: generic type name must be database table name with "Dto" prefix
        /// </summary>
        void UpdateEntity<T>(T entity) where T : IDtoEntity;

        void UpdateTask(DtoTask task, params DtoOperation[] bindedOpers);

        /// <summary>
        /// Deletes generic-type entity in repository.
        /// WARNING: generic type name must be database table name with "Dto" prefix
        /// </summary>
        void DeleteEntity<T>(int id) where T : IDtoEntity;

        List<int> UpdateOperInstancesAndGetIds();
        List<int> UpdateTaskInstancesAndGetIds();

        void CreateBase(string baseConnStr);
    }
}
