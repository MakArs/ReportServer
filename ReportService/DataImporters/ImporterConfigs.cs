using ReportService.Interfaces;

namespace ReportService.DataImporters
{
    public interface IImporterConfig : IOperationConfig
    {
    }

    public class ExcelImporterConfig : IImporterConfig
    {
        public string DataSetName { get; set; }
        public string FilePath;
        public string ScheetName;
        public bool SkipEmptyRows;
        public string[] ColumnList;
        public bool UseColumnNames;
        public int FirstDataRow;
        public int MaxRowCount;
    }

    public class DbImporterConfig : IImporterConfig
    {
        public string DataSetName { get; set; }
        public string ConnectionString;
        public string Query;
        public int TimeOut;
    }
}