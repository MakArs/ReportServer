using ReportService.Interfaces.Operations;

namespace ReportService.Operations.DataImporters.Configurations
{
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
        public string GroupNumbers;
        public bool SendVoidPackageError;
    }
}