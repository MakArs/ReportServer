namespace ReportService.Interfaces
{
    public enum RTaskType : byte
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

    public interface IRTask
    {
        void Execute(string address = null);
        string GetCurrentView();
    }
}
