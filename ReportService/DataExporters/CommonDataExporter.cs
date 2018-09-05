using System.Collections.Generic;
using ReportService.Interfaces;

namespace ReportService.DataExporters
{
    public class CommonDataExporter : IDataExporter
    {
        public List<DataType> DataTypes { get; protected set; }

        public virtual void Send(SendData sendData)
        {
        }

        public virtual void Cleanup(ICleanupSettings cleanUpSettings)
        {
        }
    }
}
