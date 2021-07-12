using Dapper.Contrib.Extensions;
using ReportService.Interfaces.Core;
using System;

namespace ReportService.Entities.Dto
{
    [Table(@"""Task""")]
    public class DtoTask : IDtoEntity
    {
        [Key]
        public long Id { get; set; }
        public string Name { get; set; }
        public string Parameters { get; set; }
        public string ParameterInfos { get; set; }
        public string DependsOn { get; set; }
        public int? ScheduleId { get; set; }
        public DateTime UpdateDateTime { get; set;}
    }
}