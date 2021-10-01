using Dapper.Contrib.Extensions;
using ReportService.Interfaces.Core;
using System;

namespace ReportService.Entities
{
    [Table(@"""TaskRequestInfo""")]
    public class TaskRequestInfo : IDtoEntity
    {
        [Key]
        public long RequestId { get; set; }
        public long TaskId { get; set; }
        public long? TaskInstanceId { get; set; }
        public string Parameters { get; set; }
        public DateTime CreateTime { get; set; }
        public DateTime UpdateTime { get; set; }
        public int Status { get; set; }
    }
}
