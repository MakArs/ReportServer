using System;
using ReportService.Interfaces.Core;

namespace ReportService.Entities
{
    public class DtoOperInstance : IDtoEntity
    {
        public int Id;
        public int TaskInstanceId;
        public int OperationId;
        public DateTime StartTime;
        public int Duration;
        public int State;
        public byte[] DataSet;
        public string ErrorMessage;
    }
}