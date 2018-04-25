namespace ReportService.Interfaces
{
    public enum RReportType : byte
    {
        Common = 1,
        Custom = 2
    }

    public enum InstanceState
    {
        InProcess = 1,
        Success = 2,
        Failed = 3
    }

    public class RSchedule
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Schedule { get; set; }
    }

    public class RRecepientGroup
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Addresses { get; set; }

        public string[] GetAddresses()
        {
            return Addresses.Split(';');
        }
    }

    public class RReport
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string ConnectionString { get; set; }
        public string ViewTemplate { get; set; }
        public string Query { get; set; }
        public int ReportType { get; set; }
        public int QueryTimeOut { get; set; } //seconds
    }

    public interface IRTask
    {
        void Execute(string address = null);
        string GetCurrentView();
    }
}
