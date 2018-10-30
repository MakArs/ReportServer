using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;
using OfficeOpenXml;
using RazorEngine;
using RazorEngine.Configuration;
using RazorEngine.Templating;
using ReportService.Extensions;
using ReportService.Interfaces;
using ReportService.Interfaces.Core;

namespace ReportService.DataExporters.ViewExecutors
{
    public class CommonViewExecutor : IViewExecutor
    {
        public virtual string ExecuteHtml(string viewTemplate, string json)
        {
            string date = $"{DateTime.Now:dd.MM.yy HH:mm:ss}";

            TemplateServiceConfiguration templateConfig =
                new TemplateServiceConfiguration
                {
                    DisableTempFileLocking = true,
                    CachingProvider = new DefaultCachingProvider(t => { })
                };

            var serv = RazorEngineService.Create(templateConfig);
            Engine.Razor = serv;
            Engine.Razor.Compile(viewTemplate, "somekey");
            JArray jObj = JArray.Parse(json);

            if (!jObj.Any()) return "No information obtained by query";

            List<string> headers = new List<string>();

            foreach (JProperty p in JObject.Parse(jObj.First.ToString()).Properties())
                headers.Add(p.Name);

            List<List<JToken>> content = new List<List<JToken>>();
            foreach (JObject j in jObj.Children<JObject>())
            {
                List<JToken> prop = new List<JToken>();
                foreach (JProperty p in j.Properties()) prop.Add(p.Value);

                content.Add(prop);
            }

            var model = new {Headers = headers, Content = content, Date = date};

            return Engine.Razor.Run("somekey", null, model);
        }

        public virtual string ExecuteTelegramView(string json, string reportName = "Отчёт")
        {
            JArray jObj = JArray.Parse(json);

            List<List<JToken>> content = new List<List<JToken>>();
            foreach (JObject j in jObj.Children<JObject>())
            {
                List<JToken> prop = new List<JToken>();
                foreach (JProperty p in j.Properties()) prop.Add(p.Value);

                content.Add(prop);
            }

            var tmRep = $@"*{reportName}*" + Environment.NewLine;
            foreach (var prop in content)
            {
                for (var i = 0; i < prop.Count; ++i)
                {
                    tmRep = i == 0 ? tmRep.Insert(tmRep.Length, Environment.NewLine + $"{prop[i]}")
                        : tmRep.Insert(tmRep.Length, $"|{prop[i]}");
                }
            }

            return tmRep;
        }

        public ExcelPackage ExecuteXlsx(string json, string reportName)
        {
            var pack = new ExcelPackage();
            var ws = pack.Workbook.Worksheets.Add(reportName);

            JArray jObj = JArray.Parse(json);

            var propNum = 0;
            var props = JObject.Parse(jObj.First.ToString()).Properties();
            foreach (JProperty p in props)
                ws.Cells[1, ++propNum].Value = p.Name;

            using (ExcelRange rng = ws.Cells[1, 1, 1, propNum])
            {
                rng.Style.Font.Bold = true;
                rng.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                rng.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGray);
            }

            int i = 0;
            foreach (JObject row in jObj.Children<JObject>())
            {
                i++;
                int j = 0;
                foreach (JProperty p in row.Properties())
                {
                    j++;
                    ws.Cells[i + 1, j].SetJValue( p.Value );
                }
            }

            ws.Cells[1, 1, i, propNum].AutoFitColumns();
            return pack;
        }
    } //class
}
