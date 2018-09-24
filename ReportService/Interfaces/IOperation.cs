using System;

namespace ReportService.Interfaces
{
    public interface IOperation
    {
        int Id { get; set; }
        bool IsDefault { get; set; }
        int Number { get; set; }
        string Name { get; set; }
        string DataSetName { get; set; }
    }

    public interface IOperationConfig
    {
        string DataSetName { get; set; }
    }

    public interface IDataExporter : IOperation
    {
        bool RunIfVoidDataSet { get; set; }
        void Send(string dataSet);
        void Cleanup(ICleanupSettings cleanUpSettings);
    }

    public interface ICleanupSettings
    {
        DateTime KeepingTime { get; set; }
    }

    public interface IDataImporter : IOperation
    {
        string Execute();
    }
}