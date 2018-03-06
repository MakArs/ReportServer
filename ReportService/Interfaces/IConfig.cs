using ReportService.Implementations;
using System.Collections.Generic;

namespace ReportService.Interfaces
{
    public interface IConfig
    {
        int CreateInstance(int ataskID, string ajson, string ahtml, double aduration, string asuccess, int atryNumber);
        void UpdateInstance(int ainstanceID, string ajson, string ahtml, double aduration, string astate, int atryNumber);
        void Reload();
        List<DTO_Task> GetTasks();
    }
}
