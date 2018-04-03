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

    public enum Schedule
    {
        motuwethfr2230=1,
        su2230=2
    }

    public interface IRTask
    {
        void Execute(string address = null);
    }
}
