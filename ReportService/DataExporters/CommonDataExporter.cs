using ReportService.Interfaces;

namespace ReportService.DataExporters
{
    public class CommonDataExporter : IDataExporter
    {
        public int Id { get; set; }
        public int Number { get; set; }
        public string DataSetName { get; set; }

        public virtual void Send(string dataSet)
        {
        }

        public virtual void Cleanup(ICleanupSettings cleanUpSettings)
        {
        }
    }
}
