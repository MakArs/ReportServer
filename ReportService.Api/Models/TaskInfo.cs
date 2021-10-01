using System.Collections.Generic;
using ReportService.Entities.Dto;

namespace ReportService.Api.Models
{
    public class TaskInfo
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public ParameterInfo[] ParameterInfos { get; set; }

        public TaskInfo(long id, string name, List<ParameterInfo> parameterInfos)
        {
            this.Id = id;
            this.Name = name;
            this.ParameterInfos = parameterInfos.ToArray();
        }
    }
}