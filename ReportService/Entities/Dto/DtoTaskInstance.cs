using System;
using ReportService.Interfaces.Core;

namespace ReportService.Entities.Dto
{
    public class DtoTaskInstance : IDtoEntity
    {
        public long Id;
        public long TaskId;
        public DateTime StartTime;
        public int Duration;
        public int State;
    }
}