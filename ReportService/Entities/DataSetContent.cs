using System.Collections.Generic;

namespace ReportService.Entities
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