namespace ReportService.Interfaces
{
    public interface IViewExecutor
    {
        string ExecuteHtml(string viewTemplate, string json);
        string ExecuteTelegramView(string json, string reportName = "Отчёт");
    }
}