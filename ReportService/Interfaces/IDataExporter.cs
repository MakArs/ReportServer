namespace ReportService.Interfaces
{
    public interface IDataExporter
    {
        string DataSetName { get; set; }
        void Send(string dataSet);
        void Cleanup(ICleanupSettings cleanUpSettings);
    }
}