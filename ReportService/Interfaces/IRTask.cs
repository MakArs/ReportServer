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

    public interface IRTask
    {
        void Execute(string address = null);
    }
}
