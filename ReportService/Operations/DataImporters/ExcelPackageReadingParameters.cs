namespace ReportService.Operations.DataImporters
{
    public class ExcelPackageReadingParameters
    {
        public string SheetName;
        public bool SkipEmptyRows;
        public string[] ColumnList;
        public bool UseColumnNames;
        public int FirstDataRow;
        public int MaxRowCount;
    }
}
