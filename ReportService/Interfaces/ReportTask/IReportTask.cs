using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ReportService.Entities;
using ReportService.Interfaces.Operations;

namespace ReportService.Interfaces.ReportTask
{
    public enum RReportType : byte
    {
        Common = 1,
        Custom = 2
    }

    public interface IReportTask
    {
        int Id { get; }
        string Name { get; }
        DtoSchedule Schedule { get; }
        DateTime LastTime { get; }
        List<IOperation> Operations { get; set; }
        Dictionary<string, object> Parameters { get; set; }
        Dictionary<int, long> DependsOn { get; set; }

        IReportTaskRunContext GetCurrentContext(bool isDefault);
        void Execute(IReportTaskRunContext context);
        void UpdateLastTime();
        Task<string> GetCurrentViewAsync(IReportTaskRunContext context);
        void SendDefault(IReportTaskRunContext context, string mailAddress);
    }
}