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
        public string Execute(string viewTemplate, string json)
        {
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

            var model = new { Headers = headers, Content = content };

            return Engine.Razor.Run("somekey", null, model);
        }
    }
}


//        public ViewExecutor() { }
//        string htmlWrap = @"<!DOCTYPE html>
//<html>
//<head>
//<title>ReportTable</title>
//<style>
//table {
//    border-collapse: collapse;
//    width: 100%;
//}
//th, td
//{
//    border: 1px solid Black;
//	padding: 10px;
//}
//</style>
//</head>
//<body>
//<table>
//<tr>";
            //StringBuilder htmlCode = new StringBuilder(htmlWrap);

//            foreach (JProperty p in JObject.Parse(jObj.First.ToString()).Properties())
//                htmlCode.AppendLine($@"<th>{p.Name}</th>");

//            htmlCode.AppendLine(@"</tr>");

//            foreach (JObject j in jObj.Children<JObject>())
//            {
//                htmlCode.AppendLine("<tr>");

//                foreach (JProperty p in j.Properties())
//                    htmlCode.AppendLine(($@"<td>{p.Value}</td>"));

//                htmlCode.AppendLine("</tr>");
//            }

//            htmlCode.AppendLine(@"</table> 
//</body>
//</html>");
