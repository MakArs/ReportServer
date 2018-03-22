using ReportService.Implementations;
using System.Collections.Generic;

namespace ReportService.Interfaces
{
    public interface IConfig
    {
        int CreateInstance(int ataskID, string ajson, string ahtml, double aduration, string asuccess, int atryNumber);
        void UpdateInstance(int ainstanceID, string ajson, string ahtml, double aduration, string astate, int atryNumber);
        void Reload();
        void CreateBase(string abaseConnStr);
        List<DTO_Task> GetTasks();
        void UpdateTask(int ataskID, DTO_Task atask);
        void DeleteTask(int ataskID);
        int CreateTask(DTO_Task atask);
    }
}
