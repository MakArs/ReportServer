using ReportService.Implementations;
using System.Collections.Generic;

namespace ReportService.Interfaces
{
    public interface IConfig
    {
        int SaveInstance(int taskID, string json, string html);
        void Reload();
        List<DTO_Task> GetTasks();
    }
}
