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
                    rng.Value = ((DateTime)token).ToOADate();
                    rng.Style.Numberformat.Format = "mm/dd/yyyy hh:mm:ss";
                    break;
                case JTokenType.Integer:
                    rng.Value = (long)token;
                    break;
                case JTokenType.Float:
                    rng.Value = (double)token;
                    break;
                default:
                    rng.Value = token.ToString();
                    break;
            }
        }
    }
}
