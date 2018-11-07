using OfficeOpenXml;

namespace ReportService.Interfaces.Core
{
    public interface IViewExecutor
    {
        string ExecuteHtml(string viewTemplate, OperationPackage package);
        string ExecuteTelegramView(OperationPackage package, string reportName = "Отчёт");
        ExcelPackage ExecuteXlsx(OperationPackage package, string reportName);
    }
}