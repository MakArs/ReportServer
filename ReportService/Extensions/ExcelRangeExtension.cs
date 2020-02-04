using Newtonsoft.Json.Linq;
using OfficeOpenXml;
using System;

namespace ReportService.Extensions
{
    public static class ExcelRangeExtension
    {
        public static void SetJValue(this ExcelRange rng, JToken token)
        {
            switch (token.Type)
            {
                case JTokenType.Date:
                    rng.Value = ((DateTime) token).ToOADate();
                    rng.Style.Numberformat.Format = "dd/mm/yyyy hh:mm:ss";
                    break;
                case JTokenType.Integer:
                    rng.Value = (long) token;
                    break;
                case JTokenType.Float:
                    rng.Value = (double) token;
                    break;
                default:
                    rng.Value = token.ToString();
                    break;
            }
        }

        public static void SetObjValue(this ExcelRange rng, object value, string dateFormat)
        {
            switch (value)
            {
                case int intValue:
                    rng.Value = intValue;
                    break;

                case short shortValue:
                    rng.Value = shortValue;
                    break;

                case byte byteValue:
                    rng.Value = byteValue;
                    break;

                case double doublevValue:
                    rng.Value = doublevValue;
                    break;

                case decimal decimalValue:
                    rng.Value = decimalValue;
                    break;

                case long longValue:
                    rng.Value = longValue;
                    break;

                case bool boolValue:
                    rng.Value = boolValue;
                    break;

                case DateTime dateTimeValue:
                    rng.Value = dateTimeValue;
                    rng.Style.Numberformat.Format =
                        dateTimeValue.TimeOfDay == TimeSpan.Zero //todo: change logic in some way
                            ? "dd.mm.yyyy"
                            : "dd.mm.yyyy HH:mm:ss";
                    break;

                default:
                    rng.Value = value?.ToString();
                    break;
            }
        }
    }
}