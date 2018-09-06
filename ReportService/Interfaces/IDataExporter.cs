using System;

namespace ReportService.Interfaces
{
    public interface IDataExporter
    {
        string DataSetName { get; set; }
        void Send(string dataSet);
        void Cleanup(ICleanupSettings cleanUpSettings);
    }

    public interface IExporterConfig
    {
        string DataSetName { get; set; }
    }

    public interface ICleanupSettings
    {
        DateTime KeepingTime { get; set; }
    }
}