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
        public int? RecepientGroupId { get; set; }
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

    public class DtoRecepientGroup
    {
        public int Id { get; set; } = 0;
        public string Name { get; set; } = "";
        public string Addresses { get; set; } = "";
    }

    public interface IRepository
    {
        List<DtoInstance> GetInstancesByTaskId(int taskId);
        List<DtoInstanceCompact> GetCompactInstancesByTaskId(int taskId);
        DtoInstance GetInstanceById(int id);
        List<DtoInstanceCompact> GetAllCompactInstances();
        List<DtoSchedule> GetAllSchedules();
        List<DtoRecepientGroup> GetAllRecepientGroups();
        List<DtoTask> GetTasks();

        void CreateBase(string baseConnStr);
        int CreateTask(DtoTask task);
        int CreateInstance(DtoInstanceCompact instance, DtoInstanceData data);
        void UpdateTask(DtoTask task);
        void UpdateInstance(DtoInstanceCompact instance, DtoInstanceData data);
        void DeleteTask(int taskId);
        void DeleteInstance(int instanceId);
    }
}
