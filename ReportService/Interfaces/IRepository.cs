using System;
using System.Collections.Generic;

namespace ReportService.Interfaces
{
    public class DtoRecepientGroup
    {
        public int Id { get; set; }
        public string Name { get; set; } 
        public string Addresses { get; set; } 
    }

    public class DtoSchedule
    {
        public int Id { get; set; }
        public string Name { get; set; } 
        public string Schedule { get; set; } 
    }

    public class DtoReport
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string ConnectionString { get; set; }
        public string ViewTemplate { get; set; }
        public string Query { get; set; }
        public int ReportType { get; set; }
        public int QueryTimeOut { get; set; } //seconds
    }

    public class DtoTask
    {
        public int Id { get; set; }
        public int ReportId { get; set; }
        public int? ScheduleId { get; set; }
        public int? RecepientGroupId { get; set; }
        public int TryCount { get; set; }
        public bool HasHtmlBody { get; set; }
        public bool HasJsonAttachment { get; set; }
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
        public byte[]  ViewData { get; set; }
    }


    public interface IRepository
    {
        List<DtoRecepientGroup> GetAllRecepientGroups();
        List<DtoSchedule> GetAllSchedules();
        List<DtoReport> GetAllReports();
        List<DtoTask> GetAllTasks();
        List<DtoInstance> GetAllInstances();
        List<DtoInstance> GetInstancesByTaskId(int taskId);
        List<DtoFullInstance> GetFullInstancesByTaskId(int taskId);
        DtoFullInstance GetFullInstanceById(int id);

        int CreateEntity<T>(T entity);
        void UpdateEntity<T>(T entity);
        void DeleteEntity<T>(int id);

        void CreateBase(string baseConnStr);
    }
}
