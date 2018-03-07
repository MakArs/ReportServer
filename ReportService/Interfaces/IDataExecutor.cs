namespace ReportService.Interfaces
{
    public interface IDataExecutor
    {
        string Execute(string aquery, int aTimeOut);
    }
}
