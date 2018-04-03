namespace ReportService.Interfaces
{
    public interface IPostMaster
    {
        void Send(string report, string[] addresses);
    }
}
