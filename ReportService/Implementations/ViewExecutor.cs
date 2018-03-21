using System;
using ReportService.Interfaces;
using Newtonsoft.Json.Linq;
using RazorEngine;
using RazorEngine.Templating;
using System.Collections.Generic;
using RazorEngine.Configuration;

namespace ReportService.Implementations
{
    public class ViewExecutor : IViewExecutor
    {
        public virtual string Execute(string viewTemplate, string json)
        {
            string date = $"{DateTime.Now:dd.MM.yy HH:mm:ss}";

            TemplateServiceConfiguration templateConfig = new TemplateServiceConfiguration();
            templateConfig.DisableTempFileLocking = true;
            templateConfig.CachingProvider = new DefaultCachingProvider(t => { });
            var serv = RazorEngineService.Create(templateConfig);
            Engine.Razor = serv;
            Engine.Razor.Compile(viewTemplate, "somekey");

            JArray jObj = JArray.Parse(json);

            List<string> headers = new List<string>();
            foreach (JProperty p in JObject.Parse(jObj.First.ToString()).Properties())
                headers.Add(p.Name);


            List<List<string>> content = new List<List<string>>();
            foreach (JObject j in jObj.Children<JObject>())
            {
                List<string> prop = new List<string>();
                foreach (JProperty p in j.Properties()) prop.Add(p.Value.ToString());

                content.Add(prop);
            }

            var model = new { Headers = headers, Content = content, Date = date };

            return Engine.Razor.Run("somekey", null, model);
        }
    }//class
}
