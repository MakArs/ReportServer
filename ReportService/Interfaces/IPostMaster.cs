namespace ReportService.Interfaces
{
    public interface IPostMaster
    {
        void Send(string[] addresses, string htmlReport = null, string jsonReport = null);
    }
}
