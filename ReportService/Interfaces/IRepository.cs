using System;
using System.Collections.Generic;

namespace ReportService.Interfaces
{
    public class DTOTask
    {
        public int Id { get; set; }
        public int ScheduleId { get; set; }
        public string ConnectionString { get; set; }
        public string ViewTemplate { get; set; }
        public string Query { get; set; }
        public string SendAddresses { get; set; }
        public int TryCount { get; set; }
        public int QueryTimeOut { get; set; } //seconds
        public int TaskType { get; set; }
    }

    public class DTOInstance
    {
        public int Id { get; set; }
        public string Data { get; set; } = "";
        public string ViewData { get; set; } = "";
        public int TaskId { get; set; }
        public DateTime StartTime { get; set; }
        public int Duration { get; set; }
        public int State { get; set; }
        public int TryNumber { get; set; }
    }

    public class DTOInstanceCompact
    {
        public int Id { get; set; }
        public int TaskId { get; set; }
        public DateTime StartTime { get; set; }
        public int Duration { get; set; }
        public int State { get; set; }
        public int TryNumber { get; set; }
    }

    public class DTOInstanceData
    {
        public int InstanceId { get; set; }
        public string Data { get; set; } = "";
        public string ViewData { get; set; } = "";
    }

    public interface IRepository
    {
        List<DTOInstance> GetInstancesByTaskId(int taskId);
        List<DTOInstanceCompact> GetCompactInstancesByTaskId(int taskId);
        DTOInstance GetInstanceById(int id);
        List<DTOInstanceCompact> GetAllCompactInstances();
        void UpdateInstance(DTOInstanceCompact instance, DTOInstanceData data);
        int CreateInstance(DTOInstanceCompact instance, DTOInstanceData data);
        void DeleteInstance(int instanceId);
        List<DTOTask> GetTasks();
        void UpdateTask(DTOTask task);
        void DeleteTask(int taskId);
        int CreateTask(DTOTask task);
        void CreateBase(string baseConnStr);
    }
}
