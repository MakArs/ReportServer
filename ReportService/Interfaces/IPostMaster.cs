namespace ReportService.Interfaces
{
    public interface IPostMaster
    {
        void Send(string reportName, string[] addresses, string htmlReport = null, string jsonReport = null);
    }
}
