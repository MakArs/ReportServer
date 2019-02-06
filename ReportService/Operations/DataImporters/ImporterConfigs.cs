using ReportService.Interfaces.Core;

namespace ReportService.Operations.DataImporters
{
    public interface IImporterConfig : IOperationConfig
    {
    }

    public class ExcelImporterConfig : IImporterConfig
    {
        public string PackageName { get; set; }
        public string FileFolder;
        public string FileName;
        public string ScheetName;
        public bool SkipEmptyRows;
        public string[] ColumnList;
        public bool UseColumnNames;
        public int FirstDataRow;
        public int MaxRowCount;
    }

    public class SshImporterConfig : IImporterConfig
    {
        public string PackageName { get; set; }
        public string Host;
        public string Login;
        public string Password;
        public string FilePath;
    }

    public class CsvImporterConfig : IImporterConfig
    {
        public string PackageName { get; set; }
        public string FileFolder;
        public string FileName;
        public string Delimiter;
    }

    public class DbImporterConfig : IImporterConfig
    {
        public string PackageName { get; set; }
        public string ConnectionString;
        public string Query;
        public int TimeOut;
    }
}