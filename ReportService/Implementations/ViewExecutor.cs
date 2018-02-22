using ReportService.Interfaces;
using Newtonsoft.Json.Linq;
using System.Text;

namespace ReportService.Implementations
{
    public class ViewExecutor : IViewExecutor
    {
        // TODO: add view templates 
        // TODO: add css Botsrap library
        public ViewExecutor() { }
        string htmlWrap = @"<!DOCTYPE html>
<html>
<head>
<title>ReportTable</title>
<style>
table {
    border-collapse: collapse;
    width: 100%;
}
th, td
{
    border: 1px solid LightGray;
	padding: 10px;
}
</style>
</head>
<body>
<table>
<tr>";
        public string Execute(int viewTemplate, string json)
        {
            StringBuilder htmlCode = new StringBuilder(htmlWrap);

            JArray jObj = JArray.Parse(json);

            foreach (JProperty p in JObject.Parse(jObj.First.ToString()).Properties())
                htmlCode.AppendLine($@"<th>{p.Name}</th>");

            htmlCode.AppendLine(@"</tr>");

            foreach (JObject j in jObj.Children<JObject>())
            {
                htmlCode.AppendLine("<tr>");

                foreach (JProperty p in j.Properties())
                    htmlCode.AppendLine(($@"<td>{p.Value}</td>"));

                htmlCode.AppendLine("</tr>");
            }

            htmlCode.AppendLine(@"</table> 
</body>
</html>");
            
            return htmlCode.ToString();
        }
    }
}
