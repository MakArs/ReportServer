using ReportService.Interfaces;
using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace ReportService.Implementations
{
    public class ViewExecutor : IViewExecutor
    {
        // TODO: add view templates 
        public ViewExecutor() { }

        // TODO: add css Botsrap library
        public string Execute(int viewTemplate, string json)
        {
            // 1. Newton: json => JObject
            JArray jArr = JArray.Parse(json);



            //foreach(JObject j in jArr)
            // 2. Generate html

            // 2.1 Wrap

            // 2.2 Table

            // 2.3 foreach children JObject string append ROW

            return "";
        }
    }
}
