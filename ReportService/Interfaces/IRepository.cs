using System;
using System.Collections.Generic;

namespace ReportService.Interfaces
{
    public class DtoOper
    {
        public int Id { get; set; }
        public string Type { get; set; }
        public string Name { get; set; }
        public string Config { get; set; }
    }

    public class DtoRecepientGroup
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Addresses { get; set; }
        public string AddressesBcc { get; set; }
    }

    public class DtoTelegramChannel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public long ChatId { get; set; }
        public int Type { get; set; } //from nuget types enum
    }

    public class DtoSchedule
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Schedule { get; set; }
    }

    // public int    QueryTimeOut     { get; set; } //seconds

    public class DtoTask
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int? ScheduleId { get; set; }
    }

    public class DtoTaskOper
    {
        public int Id { get; set; }
        public int Number { get; set; }
        public int TaskId { get; set; }
        public int ConfigId { get; set; }
    }

    public class DtoTaskInstance
    {
        public int Id { get; set; }
        public int TaskId { get; set; }
        public DateTime StartTime { get; set; }
        public int Duration { get; set; }
        public int State { get; set; }
    }

    public class DtoOperInstance
    {
        public int Id { get; set; }
        public int TaskInstanceId { get; set; }
        public int OperId { get; set; }
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
        List<T> GetListEntitiesByDtoType<T>() where T : new();

        /// <summary>
        /// Creates generic-type entity in repository.
        /// WARNING: generic type name must be database table name with "Dto" prefix
        /// </summary>
        int CreateEntity<T>(T entity);

        /// <summary>
        /// Updates generic-type entity in repository.
        /// WARNING: generic type name must be database table name with "Dto" prefix
        /// </summary>
        void UpdateEntity<T>(T entity);

        void DeleteEntity<T>(int id);

        void CreateBase(string baseConnStr);
    }
}
