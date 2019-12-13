using OfficeOpenXml;

namespace ReportService.Interfaces.Core
{
    public interface IViewExecutor
    {
        string ExecuteHtml(string viewTemplate, OperationPackage package);
        string ExecuteTelegramView(OperationPackage package, string reportName = "Отчёт", bool useAllSets = false);
        ExcelPackage ExecuteXlsx(OperationPackage package, string reportName, bool useAllSets = false);
        byte[] ExecuteCsv(OperationPackage package, string delimiterr = ";", bool useAllSets = false);
    }
}