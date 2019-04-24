using ReportService.Interfaces.Core;
using ReportService.Interfaces.Operations;

namespace ReportService.Operations.DataExporters.Configurations
{
    public interface IExporterConfig : IOperationConfig
    {
        bool RunIfVoidPackage { get; set; }
    }
}
