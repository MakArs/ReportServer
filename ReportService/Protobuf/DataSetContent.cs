using System.Collections.Generic;

namespace ReportService.Protobuf
{
    public class DataSetContent
    {
        public string Name;
        public List<string> Headers;
        public List<List<object>> Rows;
        public List<int> GroupColumns;
        public List<OrderSettings> OrderColumns;
    }
}
