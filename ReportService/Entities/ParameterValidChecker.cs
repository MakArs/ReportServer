using System;
using System.Collections.Generic;
using System.Linq;
using ReportService.Entities.Dto;

namespace ReportService.Entities
{
    public class ParameterChecker
    {
        public List<string> Error { get; } = new List<string>();
        public object Value { get; set; }
        
        public ParameterMapping MainCheck(ParameterInfo parameterInfo, TaskParameter taskParameter, List<TaskParameter> linkedParameters)
        {
            if (!IsRequiredCheck(parameterInfo, taskParameter))
                return new ParameterMapping(parameterInfo, taskParameter, Error, Value);

            TypeCheck(parameterInfo, taskParameter);

            if (parameterInfo.Validation?.ValidationRules == null)
            {
                return new ParameterMapping(parameterInfo, taskParameter, Error, Value);
            }

            foreach (var validationRule in parameterInfo.Validation?.ValidationRules)
            {
                switch (validationRule)
                {
                    case DateRangeValidationRule drValidationRule:
                        DateRangeValidationCheck(drValidationRule, taskParameter, linkedParameters);
                        break;
                }
            }

            return new ParameterMapping(parameterInfo, taskParameter, Error, Value);
        }

        private bool IsRequiredCheck(ParameterInfo parameterInfo, TaskParameter taskParameter)
        {
            if (taskParameter != null)
                return true;

            if (!parameterInfo.IsRequired)
            {
                return true;
            }

            Error.Add($"The required parameter with name:{parameterInfo.Name} is missing.");
            return false;
        }

        private void TypeCheck(ParameterInfo parameterInfo, TaskParameter taskParameter)
        {
            var paramType = parameterInfo.Type;
            switch (paramType)
            {
                case "bigint":
                    if (!long.TryParse(taskParameter.Value, out _))
                        Error.Add($"Wrong value input. TypeError in parameter: {taskParameter.Name}.");
                    else
                        Value = Convert.ToInt64(taskParameter.Value);
                    break;

                case "int":
                    if (!int.TryParse(taskParameter.Value, out _))
                        Error.Add($"Wrong value input. TypeError in parameter: {taskParameter.Name}.");
                    else
                        Value = Convert.ToInt32(taskParameter.Value);
                    break;

                case "datetime":
                    if (!DateTime.TryParse(taskParameter.Value, out _))
                        Error.Add($"Wrong value input. TypeError in parameter: {taskParameter.Name}.");
                    else
                        Value = Convert.ToDateTime(taskParameter.Value);
                    break;

                case "string":
                    if (taskParameter.Value.GetType() != typeof(string))
                        Error.Add($"Wrong value input. TypeError in parameter: {taskParameter.Name}.");
                    else
                        Value = taskParameter.Value;
                    break;

                default:
                    Error.Add($"Wrong type of parameter: {parameterInfo.Name}.");
                    break;
            }
        }
        
        private void DateRangeValidationCheck(DateRangeValidationRule dateRangeValidationRule, TaskParameter taskParameter,
            IEnumerable<TaskParameter> linkedParameters)
        {
            var linkedValue = linkedParameters
                .FirstOrDefault(
                    x => x.Name.ToLower().Contains(dateRangeValidationRule.LinkedParameterName.ToLower())
                )?.Value;

            if (linkedValue == null)
            {
                Error.Add($@"The linked parameter value is not set.");
                return;
            }
            
            if (!DateTime.TryParse(linkedValue, out _))
            {
                Error.Add(@"The linked parameter value has wrong type.");
                return;
            }

            var dateDiff = Convert.ToDateTime(linkedValue)
                .Subtract(Convert.ToDateTime(taskParameter.Value)).Days;
            
            if (taskParameter.Name.ToLower().Contains("repparfrom") && dateDiff >= dateRangeValidationRule.MaxDays)
                Error.Add($"Time period error. The time period is to big ({dateDiff} days). It can be more than {dateRangeValidationRule.MaxDays}");
            
            if (taskParameter.Name.ToLower().Contains("repparto") && dateDiff < 0)
                Error.Add($"Time period error. Time from point can not be late than time to point.");
        }
    }
}
