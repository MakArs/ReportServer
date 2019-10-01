using System;
using ReportService.Interfaces.Core;

namespace ReportService.Entities
{
    public class DtoOperInstance : IDtoEntity
    {
        public long Id;
        public long TaskInstanceId;
        public long OperationId;
        public DateTime StartTime;
        public int Duration;
        public int State;
        public byte[] DataSet;
        public string ErrorMessage;
    }
}