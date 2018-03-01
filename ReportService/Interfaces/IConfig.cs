using ReportService.Implementations;
using System.Collections.Generic;

namespace ReportService.Interfaces
{
    public interface IConfig
    {
        int CreateInstance(int ataskID, string ajson, string ahtml, double aduration, int asuccess, int atryNumber);
        void Reload();
        List<DTO_Task> GetTasks();
    }
}
