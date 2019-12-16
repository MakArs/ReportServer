namespace ReportService.Interfaces.Operations
{
    public interface IExporterConfig : IOperationConfig
    {
        bool RunIfVoidPackage { get; set; }
    }
}
