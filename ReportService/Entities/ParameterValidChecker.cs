using System;
using System.Collections.Generic;
using System.Linq;
using ReportService.Entities.Dto;

namespace ReportService.Entities
{
    public class ParameterChecker
    {
        public ParameterMapping Check(ParameterInfo parameterInfo, TaskParameter taskParameter, List<TaskParameter> linkedParameters)
        {
            var errors = new List<string>();

            var isRequired = IsRequiredCheck(parameterInfo, taskParameter);
            if (isRequired != null)
            {
                errors.Add(isRequired);
                return new ParameterMapping(parameterInfo, taskParameter, errors, null);
            }
                
            errors.AddRange(TypeCheck(parameterInfo, taskParameter));

            if (parameterInfo.Validation?.ValidationRules == null)
            {
                return new ParameterMapping(parameterInfo, taskParameter, errors, taskParameter.Value);
            }

            foreach (var validationRule in parameterInfo.Validation?.ValidationRules)
            {
                switch (validationRule)
                {
                    case DateRangeValidationRule drValidationRule:
                        errors.AddRange(DateRangeValidationCheck(drValidationRule, taskParameter, linkedParameters));
                        break;
                }
            }

            return new ParameterMapping(parameterInfo, taskParameter, errors, taskParameter.Value);
        }

        private string IsRequiredCheck(ParameterInfo parameterInfo, TaskParameter taskParameter)
        {
            if (taskParameter != null)
                return null;

            if (!parameterInfo.IsRequired)
            {
                return null;
            }

            return $"The required parameter with name:{parameterInfo.Name} is missing.";
        }

        private List<string> TypeCheck(ParameterInfo parameterInfo, TaskParameter taskParameter)
        {
            var paramType = parameterInfo.Type;
            var errors = new List<string>();
            switch (paramType)
            {
                case "bigint":
                    if (!long.TryParse(taskParameter.Value, out _))
                        errors.Add($"Wrong value input. TypeError in parameter: {taskParameter.Name}.");
                    break;

                case "int":
                    if (!int.TryParse(taskParameter.Value, out _))
                        errors.Add($"Wrong value input. TypeError in parameter: {taskParameter.Name}.");
                    break;

                case "datetime":
                    if (!DateTime.TryParse(taskParameter.Value, out _))
                        errors.Add($"Wrong value input. TypeError in parameter: {taskParameter.Name}.");

                    break;

                case "string":
                    if (taskParameter.Value.GetType() != typeof(string))
                        errors.Add($"Wrong value input. TypeError in parameter: {taskParameter.Name}.");
                    break;

                default:
                    errors.Add($"Wrong type of parameter: {parameterInfo.Name}.");
                    break;
                
            }

            return errors;
        }
        
        private List<string> DateRangeValidationCheck(DateRangeValidationRule dateRangeValidationRule, TaskParameter taskParameter,
            IEnumerable<TaskParameter> linkedParameters)
        {
            var errors = new List<string>();
            var linkedValue = linkedParameters
                .FirstOrDefault(
                    x => x.Name.ToLower().Contains(dateRangeValidationRule.LinkedParameterName.ToLower())
                )?.Value;

            if (linkedValue == null)
            {
                errors.Add($@"The linked parameter value is not set.");
            }
            
            if (!DateTime.TryParse(linkedValue, out _))
            {
                errors.Add(@"The linked parameter value has wrong type.");
                return errors;
            }

            var dateDiff = Convert.ToDateTime(linkedValue)
                .Subtract(Convert.ToDateTime(taskParameter.Value)).Days;
            
            if (taskParameter.Name.ToLower().Contains("repparfrom") && dateDiff >= dateRangeValidationRule.MaxDays)
                errors.Add($"Time period error. The time period is to big ({dateDiff} days). It can be more than {dateRangeValidationRule.MaxDays}");
            
            if (taskParameter.Name.ToLower().Contains("repparto") && dateDiff < 0)
                errors.Add($"Time period error. Time from point can not be late than time to point.");

            return errors;
        }
    }
}
