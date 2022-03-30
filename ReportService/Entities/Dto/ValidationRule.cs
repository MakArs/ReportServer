using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace ReportService.Entities.Dto
{
    public class ValidationRule
    {
    }
    
    public class ValidationRuleConverter : JsonConverter
    {
        public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }
        
        public override object? ReadJson(JsonReader reader, Type objectType, object? existingValue,
            JsonSerializer serializer)
        {
            var jObject = JObject.Load(reader);
            if (objectType == typeof(ValidationRule))
            {
                var obj = jObject.ToObject<DateRangeValidationRule>(serializer);
                if (obj.ValidationRuleName.Equals("DateRangeValidationRule"))
                {
                    return obj;
                }
            }
            throw new NotImplementedException();
        }

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(ValidationRule);
        }
    }
}
