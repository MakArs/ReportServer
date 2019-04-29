using System;
using ReportService.Interfaces.Core;

namespace ReportService.Entities
{
    public class DtoTaskInstance : IDtoEntity
    {
        public int Id;
        public int TaskId;
        public DateTime StartTime;
        public int Duration;
        public int State;
    }

    public enum InstanceState
    {
        InProcess = 1,
        Success = 2,
        Failed = 3,
        Canceled = 4
    }
}
