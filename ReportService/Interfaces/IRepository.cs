using System;
using System.Collections.Generic;

namespace ReportService.Interfaces
{
    public interface IDtoEntity
    {
    }

    public class DtoOperTemplate : IDtoEntity
    {
        public int Id { get; set; }
        public string ImplementationType { get; set; }
        public string Name { get; set; }
        public string ConfigTemplate { get; set; }
    }

    public class DtoRecepientGroup : IDtoEntity
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Addresses { get; set; }
        public string AddressesBcc { get; set; }
    }

    public class DtoTelegramChannel : IDtoEntity
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public long ChatId { get; set; }
        public int Type { get; set; } //from nuget types enum
    }

    public class DtoSchedule : IDtoEntity
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Schedule { get; set; }
    }

    public class DtoTask : IDtoEntity
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int? ScheduleId { get; set; }
    }

    public class DtoOperation : IDtoEntity
    {
        public int Id { get; set; }
        public int TaskId { get; set; }
        public int Number { get; set; }
        public string Name { get; set; }
        public string ImplementationType { get; set; }
        public bool IsDefault { get; set; }
        public string Config { get; set; }
        public bool IsDeleted { get; set; }
    }

    public class DtoTaskInstance : IDtoEntity
    {
        public int Id { get; set; }
        public int TaskId { get; set; }
        public DateTime StartTime { get; set; }
        public int Duration { get; set; }
        public int State { get; set; }
    }

    public class DtoOperInstance : IDtoEntity
    {
        public int Id { get; set; }
        public int TaskInstanceId { get; set; }
        public int OperationId { get; set; }
        public DateTime StartTime { get; set; }
        public int Duration { get; set; }
        public int State { get; set; }
        public byte[] DataSet { get; set; }
        public string ErrorMessage { get; set; }
    }

    public interface IRepository
    {
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

        void CreateBase(string baseConnStr);
    }
}
