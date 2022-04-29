using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using NUnit.Framework;
using ReportService.Entities;
using ReportService.Entities.Dto;
using Shouldly;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace ReportService.Tests.Entities
{
    [TestFixture]
    public class ParameterValidCheckerTests
    {
        private ParameterInfo CreateInputParameterInfo(string jsonParameterInfo)
        {
            return JsonConvert.DeserializeObject<ParameterInfo>(jsonParameterInfo, new ValidationRuleConverter());
        }

        [Test]
        public void ShouldMapDateTimeParameters()
        {
            var repParFrom = new ParameterInfo
            {
                Name = "@RepParFrom",
                Type = "DateTime",
                IsRequired = true,
                Description = "uTest",
                DefaultValue = "",
                Validation = new Validation {ValidationRules = new List<ValidationRule> {new DateRangeValidationRule{ValidationRuleName = "DateRangeValidationRule", LinkedParameterName = "@RepParto", MaxDays = 62}}}
            };
            
            var userRepParFrom = new TaskParameter
            {
                Name = "@RepParFrom",
                Value = "2021-06-01"
            };

            var repParTo = new TaskParameter
            {
                Name = "@RepParTo",
                Value = "2021-06-03"
            };

            var expectedValue =
                new ParameterMapping(repParFrom, userRepParFrom, new List<string>(), userRepParFrom.Value);

            var checker = new ParameterChecker();
            checker.Check(repParFrom, userRepParFrom, new List<TaskParameter> {repParTo})
                .ShouldBeEquivalentTo(expectedValue);
        }
        
        
        [TestCase(
            "{" +
                "\"Name\":\"@RepParFrom\"," +
                "\"Type\":\"DateTime\"," +
                "\"IsRequired\":true," +
                "\"Description\":\"uTest\"," +
                "\"DefaultValue\":\"\"," +
                "\"Validation\":" +
                    "{\"ValidationRules\":" +
                        "[{\"ValidationRuleName\":\"DateRangeValidationRule\"," +
                        "\"LinkedParameterName\":\"@RepParto\"," +
                        "\"MaxDays\":62}]}}",
            "[" +
                "{\"Name\": \"@RepParFrom\", \"Value\": \"21\"}," +
                "{\"Name\": \"@RepParFrom\", \"Value\": \"string\"}," +
                "{\"Name\": \"@RepParFrom\", \"Value\": \"(92233720360\"}]",
            "[{\"Name\": \"@RepParTo\", \"Value\": \"2021-06-03\"}]",
            "Wrong value input. TypeError in parameter: @RepParFrom.",
            TestName = "DateTimeTypeTest"
        )]
        [TestCase(
            "{" +
                "\"Name\":\"@RepParFrom\"," +
                "\"Type\":\"int\"," +
                "\"IsRequired\":true," +
                "\"Description\":\"uTest\"," +
                "\"DefaultValue\":\"\"}",
            "[" +
                "{\"Name\": \"@RepParFrom\", \"Value\": \"2021-06-03\"}," +
                "{\"Name\": \"@RepParFrom\", \"Value\": \"string\"}," +
                "{\"Name\": \"@RepParFrom\", \"Value\": \"(92233720360\"}]",
            null,
            "Wrong value input. TypeError in parameter: @RepParFrom.",
            TestName = "IntTypeTest"
        )]
        [TestCase(
            "{" +
                "\"Name\":\"@RepParFrom\"," +
                "\"Type\":\"bigint\"," +
                "\"IsRequired\":true," +
                "\"Description\":\"uTest\"," +
                "\"DefaultValue\":\"\"}",
            "[" +
                "{\"Name\": \"@RepParFrom\", \"Value\": \"2021-06-03\"}," +
                "{\"Name\": \"@RepParFrom\", \"Value\": \"string\"}]",
            null,
            "Wrong value input. TypeError in parameter: @RepParFrom.",
            TestName = "BigintTypeTest"
        )]
        [TestCase(
            "{" +
                "\"Name\":\"@RepParFrom\"," +
                "\"Type\":\"DateTime\"," +
                "\"IsRequired\":true," +
                "\"Description\":\"uTest\"," +
                "\"DefaultValue\":\"\"," +
                "\"Validation\":" +
                    "{\"ValidationRules\":" +
                        "[{\"ValidationRuleName\":\"DateRangeValidationRule\"," +
                        "\"LinkedParameterName\":\"@RepParto\"," +
                        "\"MaxDays\":62}]}}",
            "[{\"Name\": \"@RepParFrom\", \"Value\": \"2021-06-01\"}]",
            "[{\"Name\": \"@RepPar\", \"Value\": \"2021-06-03\"}]",
            "The linked parameter value is not set.",
            TestName = "LinkedParameterNotSet"
        )]
        [TestCase(
            "{" +
                "\"Name\":\"@RepParFrom\"," +
                "\"Type\":\"DateTime\"," +
                "\"IsRequired\":true," +
                "\"Description\":\"uTest\"," +
                "\"DefaultValue\":\"\"," +
                "\"Validation\":" +
                    "{\"ValidationRules\":" +
                        "[{\"ValidationRuleName\":\"DateRangeValidationRule\"," +
                        "\"LinkedParameterName\":\"@RepParto\"," +
                        "\"MaxDays\":62}]}}",
            "[{\"Name\": \"@RepParFrom\", \"Value\": \"2021-06-01\"}]",
            "[{\"Name\": \"@RepParTo\", \"Value\": \"12\"}]",
            "The linked parameter value has wrong type.",
            TestName = "LinkedParameterWrongType"
        )]
        [TestCase(
            "{" +
                "\"Name\":\"@RepParFrom\"," +
                "\"Type\":\"DateTime\"," +
                "\"IsRequired\":true," +
                "\"Description\":\"uTest\"," +
                "\"DefaultValue\":\"\"," +
                "\"Validation\":" +
                    "{\"ValidationRules\":" +
                        "[{\"ValidationRuleName\":\"DateRangeValidationRule\"," +
                        "\"LinkedParameterName\":\"@RepParto\"," +
                        "\"MaxDays\":2}]}}",
            "[{\"Name\": \"@RepParFrom\", \"Value\": \"2021-06-01\"}]",
            "[{\"Name\": \"@RepParTo\", \"Value\": \"2021-06-04\"}]",
            "Time period error. The time period is to big (3 days). It can be more than 2",
            TestName = "LinkedParameterIsTooBig"
        )]
        [TestCase(
            "{" +
                "\"Name\":\"@RepParFrom\"," +
                "\"Type\":\"DateTime\"," +
                "\"IsRequired\":true," +
                "\"Description\":\"uTest\"," +
                "\"DefaultValue\":\"\"," +
                "\"Validation\":" +
                    "{\"ValidationRules\":" +
                        "[{\"ValidationRuleName\":\"DateRangeValidationRule\"," +
                        "\"LinkedParameterName\":\"@RepParto\"," +
                        "\"MaxDays\":2}]}}",
            "[{\"Name\": \"@RepParFrom\", \"Value\": \"2021-06-01\"}]",
            "[{\"Name\": \"@RepParTo\", \"Value\": \"2021-05-04\"}]",
            "Time period error. Time from point can not be late than time to point.",
            TestName = "LinkedParameterEarlierThanParentParameter"
        )]
        [TestCase(
            "{" +
                "\"Name\":\"@RepParFrom\"," +
                "\"Type\":\"DateTime\"," +
                "\"IsRequired\":true," +
                "\"Description\":\"uTest\"," +
                "\"DefaultValue\":\"\"}",
            null,
            null,
            "The required parameter with name:@RepParFrom is missing.",
            TestName = "RequiredParameterIsMissing"
        )]
        public void ShouldMapParameterWithError(string jsonParameterInfo, string jsonTaskParameter,
            string jsonLinkedParameters, string expectedError)
        {
            var parameterInfo = CreateInputParameterInfo(jsonParameterInfo);

            var taskParameters = new List<TaskParameter>();
            if (jsonTaskParameter != null)
            {
                taskParameters = JsonSerializer.Deserialize<List<TaskParameter>>(jsonTaskParameter);
            }
            
            var linkedParameters = new List<TaskParameter>();
            if (jsonLinkedParameters != null)
            {
                linkedParameters = JsonSerializer.Deserialize<List<TaskParameter>>(jsonLinkedParameters);
            }

            var checker = new ParameterChecker();

            if (!taskParameters.Any())
            {
                checker.Check(parameterInfo, null, linkedParameters).Error.Contains(expectedError)
                    .ShouldBeTrue();
                return;
            }

            foreach (var invalidTaskParameter in taskParameters)
            {
                checker.Check(parameterInfo, invalidTaskParameter, linkedParameters).Error.Contains(expectedError)
                    .ShouldBeTrue();
            }
        }
    }
}
