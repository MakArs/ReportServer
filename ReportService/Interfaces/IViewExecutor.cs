namespace ReportService.Interfaces
{
    public interface IViewExecutor
    {
        string Execute(int viewTemplate, string json);
    }
}
