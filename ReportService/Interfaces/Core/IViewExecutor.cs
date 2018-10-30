using OfficeOpenXml;

namespace ReportService.Interfaces.Core
{
    public interface IViewExecutor
    {
        string ExecuteHtml(string viewTemplate, string json);
        string ExecuteTelegramView(string json, string reportName = "Отчёт");
        ExcelPackage ExecuteXlsx(string json, string reportName);
    }
}