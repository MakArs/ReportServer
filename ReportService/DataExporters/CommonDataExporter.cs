using ReportService.Interfaces;

namespace ReportService.DataExporters
{
    public class CommonDataExporter : IDataExporter
    {
        public int Id { get; set; }
        public bool IsDefault { get; set; }
        public int Number { get; set; }
        public string Name { get; set; }
        public string DataSetName { get; set; }
        public bool RunIfVoidDataSet { get; set; }

        public virtual void Send(IRTaskRunContext taskContext)
        {
        }

        public virtual void Cleanup(ICleanupSettings cleanUpSettings)
        {
        }
    }
}