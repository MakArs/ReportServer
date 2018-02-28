namespace ReportService.Interfaces
{
    public interface IViewExecutor
    {
        string Execute(string viewTemplate, string json);
    }
}
