using ReportService.Interfaces;

namespace ReportService.DataExporters
{
    public class CommonDataExporter : IDataExporter
    {
        public string DataSetName { get; set; }

        public virtual void Send(string dataSet)
        {
        }

        public virtual void Cleanup(ICleanupSettings cleanUpSettings)
        {
        }
    }
}
