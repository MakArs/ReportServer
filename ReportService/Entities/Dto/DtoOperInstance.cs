using System;
using Dapper.Contrib.Extensions;
using ReportService.Interfaces.Core;

namespace ReportService.Entities.Dto
{
    [Table(@"""OperInstance""")]
    public class DtoOperInstance : IDtoEntity
    {
        [Key]
        public long Id { get; set; }
        public long TaskInstanceId { get; set; }
        public long OperationId { get; set; }
        public DateTime StartTime { get; set; }
        public int Duration { get; set; }
        public int State { get; set; }
        public byte[] DataSet { get; set; }
        public string ErrorMessage { get; set; }
    }
}