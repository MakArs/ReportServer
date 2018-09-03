using System;
using System.Collections.Generic;

namespace ReportService.Interfaces
{
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
        public int Type { get; set; }
    }

    public class DtoSchedule
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Schedule { get; set; }
    }

    public class DtoReport
    {
        public int    Id               { get; set; }
        public string Name             { get; set; }
        public string ConnectionString { get; set; }
        public string ViewTemplate     { get; set; }
        public string Query            { get; set; }
        public int    ReportType       { get; set; }
        public int    QueryTimeOut     { get; set; } //seconds
    }

    public class DtoTask
    {
        public int Id { get; set; }
        public int ReportId { get; set; }
        public int? ScheduleId { get; set; }
        public int? RecepientGroupId { get; set; }
        public int? TelegramChannelId { get; set; }
        public int TryCount { get; set; }
        public bool HasHtmlBody { get; set; }
        public bool HasJsonAttachment { get; set; }
        public bool HasXlsxAttachment { get; set; }
    }

    public class DtoFullInstance
    {
        public int Id { get; set; }
        public byte[] Data { get; set; }
        public byte[] ViewData { get; set; }
        public int TaskId { get; set; }
        public DateTime StartTime { get; set; }
        public int Duration { get; set; }
        public int State { get; set; }
        public int TryNumber { get; set; }
    }

    public class DtoInstance
    {
        public int Id { get; set; }
        public int TaskId { get; set; }
        public DateTime StartTime { get; set; }
        public int Duration { get; set; }
        public int State { get; set; }
        public int TryNumber { get; set; }
    }

    public class DtoInstanceData
    {
        public int InstanceId { get; set; }
        public byte[] Data { get; set; }
        public byte[] ViewData { get; set; }
    }

    public class DtoExporterToTaskBinder
    {
        public int Id { get; set; }
        public int TaskId { get; set; }
        public int ConfigId { get; set; }
    }

    public class DtoExporterConfig
    {
        public int Id { get; set; }
        public string ExporterType { get; set; } //todo:change with enum?
        public string JsonConfig { get; set; }
    }

    public interface IRepository
    {
        List<DtoInstance> GetInstancesByTaskId(int taskId);
        List<DtoFullInstance> GetFullInstancesByTaskId(int taskId);
        DtoFullInstance GetFullInstanceById(int id);

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
