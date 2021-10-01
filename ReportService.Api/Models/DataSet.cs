using System;

namespace ReportService.Api.Models
{
    public class DataSet
    {
        public long OperationInstanceId { get; set; }
        public DateTime StartTime { get; set; }
        public byte[] Data { get; set; }
    }
}