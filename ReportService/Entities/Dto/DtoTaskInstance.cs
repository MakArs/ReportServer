using System;
using Dapper.Contrib.Extensions;
using ReportService.Interfaces.Core;

namespace ReportService.Entities.Dto
{
    [Table("TaskInstance")]
    public class DtoTaskInstance : IDtoEntity
    {
        [Key]
        public long Id { get; set; }
        public long TaskId { get; set; }
        public DateTime StartTime { get; set; }
        public int Duration { get; set; }
        public int State { get; set; }
    }
}