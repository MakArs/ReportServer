using System;

namespace ReportService.Interfaces
{
    public interface IOperation
    {
        int Id { get; set; }
        int Number { get; set; }
        string DataSetName { get; set; }
    }

    public interface IOperationConfig
    {
        string DataSetName { get; set; }
    }

    public interface IDataExporter : IOperation
    {
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