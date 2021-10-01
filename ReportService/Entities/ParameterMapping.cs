using System.Collections.Generic;
using ReportService.Entities.Dto;

namespace ReportService.Entities
{
    public class ParameterMapping
    {
        public ParameterInfo ParameterInfo { get; set; }
        public TaskParameter UserValue { get; set; }
        public List<string> Error { get; set; }
        public object Value { get; set; }

        public ParameterMapping(
            ParameterInfo parameterInfo, 
            TaskParameter taskParameter,
            List<string> error,
            object value
            )
        {
            this.ParameterInfo = parameterInfo;
            this.UserValue = taskParameter;
            this.Error = error;
            this.Value = value;
        }
    }
}