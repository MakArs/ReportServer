using System.Collections.Generic;
using System.Threading;
using ReportService.Entities;
using ReportService.Interfaces.Operations;

namespace ReportService.Interfaces.ReportTask
{
    public interface IReportTaskRunContext
    {
        Dictionary<string, OperationPackage> Packages { get; set; }
        List<string> PackageStates { get; set; }
        List<IOperation> OpersToExecute { get; set; }
        int TaskId { get; set; }
        DtoTaskInstance TaskInstance { get; set; }
        CancellationTokenSource CancelSource { get; set; }
        string TaskName { get; set; }
        IDefaultTaskExporter Exporter { get; set; }
        Dictionary<string, object> Parameters { get; set; }
        Dictionary<int, long> DependsOn { get; set; }
        string DataFolderPath { get; }

        byte[] GetCompressedPackage(string packageName);
        void CreateDataFolder();
        void RemoveDataFolder();
        string SetQueryParameters(List<object> parametersList, string innerString);
        string SetStringParameters(string innerString);
    }
}