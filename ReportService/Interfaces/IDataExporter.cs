using System.Collections.Generic;
using OfficeOpenXml;

namespace ReportService.Interfaces
{
    public interface IDataExporter
    {
        List<string> DataTypes { get; }
        void Send(SendData sendData);
        void Cleanup(ICleanupSettings cleanUpSettings);
    }

    public class SendData
    {
        public string JsonBaseData { get; set; }
        public string JsonEnData { get; set; }
        public string TelegramData { get; set; }
        public string HtmlData { get; set; }
        public ExcelPackage XlsxData { get; set; }
    }
}
