using System;
using System.Collections.Generic;

namespace ReportService.Interfaces
{
    public class DtoTask
    {
        public int Id { get; set; }
        public int? ScheduleId { get; set; }
        public string ConnectionString { get; set; }
        public string ViewTemplate { get; set; }
        public string Query { get; set; }
        public string SendAddresses { get; set; }
        public int TryCount { get; set; }
        public int QueryTimeOut { get; set; } //seconds
        public int TaskType { get; set; }
    }

    public class DtoInstance
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

    public class DtoInstanceCompact
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
        public string Data { get; set; }
        public string ViewData { get; set; }
    }

    public class DtoSchedule
    {
        public int Id { get; set; } = 0;
        public string Name { get; set; } = "";
        public string Schedule { get; set; } = "";
    }

    public interface IRepository
    {
        List<DtoInstance> GetInstancesByTaskId(int taskId);
        List<DtoInstanceCompact> GetCompactInstancesByTaskId(int taskId);
        DtoInstance GetInstanceById(int id);
        List<DtoInstanceCompact> GetAllCompactInstances();
        List<DtoSchedule> GetAllSchedules();
        void UpdateInstance(DtoInstanceCompact instance, DtoInstanceData data);
        int CreateInstance(DtoInstanceCompact instance, DtoInstanceData data);
        void DeleteInstance(int instanceId);
        List<DtoTask> GetTasks();
        void UpdateTask(DtoTask task);
        void DeleteTask(int taskId);
        int CreateTask(DtoTask task);
        void CreateBase(string baseConnStr);
    }
}
