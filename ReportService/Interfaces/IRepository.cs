using ReportService.Implementations;
using System.Collections.Generic;

namespace ReportService.Interfaces
{
    public interface IRepository
    {
        int CreateInstance(int taskId, string json, string html, double duration, string state, int tryNumber);
        void UpdateInstance(int instanceId, string json, string html, double duration, string state, int tryNumber);
        void CreateBase(string baseConnStr);
        List<DTOTask> GetTasks();
        void UpdateTask(int taskId, DTOTask task);
        void DeleteTask(int taskId);
        int CreateTask(DTOTask task);
    }
}
