using ReportService.Implementations;
using System.Collections.Generic;

namespace ReportService.Interfaces
{
    public interface IRepository
    {
        List<DTOInstance> GetInstances();
        void UpdateInstance(DTOInstance instance);
        int CreateInstance(DTOInstance instance);
        List<DTOTask> GetTasks();
        void UpdateTask(DTOTask task);
        void DeleteTask(int taskId);
        int CreateTask(DTOTask task);
        void CreateBase(string baseConnStr);
    }
}
