using System;
using System.Collections.Generic;
using System.Linq;
using RazorEngine;
using RazorEngine.Configuration;
using RazorEngine.Templating;
using ReportService.Interfaces.Protobuf;

namespace ReportService.Operations.DataExporters.ViewExecutors
{
    public class GroupedViewExecutor : CommonViewExecutor
    {
        public override string ExecuteHtml(string tableName, OperationPackage package)
        {
            string date = $"{DateTime.Now:dd.MM.yy HH:mm:ss}";

            TemplateServiceConfiguration templateConfig =
                new TemplateServiceConfiguration
                {
                    DisableTempFileLocking = true,
                    CachingProvider = new DefaultCachingProvider(t => { })
                };

            templateConfig.Namespaces
                .Add("ReportService.Operations.DataExporters.ViewExecutors");

            var serv = RazorEngineService.Create(templateConfig);

            if (!package.DataSets.Any()) return "No information obtained by query";

            var packageValues = packageParser.GetPackageValues(package);

            var dataSet = packageValues.First();

            var groupColumns = dataSet.GroupColumns;
            var grouping = GetMergedRows(dataSet.Rows, groupColumns, groupColumns);
            var groupedT = CreateGroupedHtmlTable(grouping);
            groupedT = groupedT.Replace("@", "&#64;"); //needed '@' symbol escaping for proper razorengine work

            string tableTemplate =
                @"<!DOCTYPE html>
<html>
<head>
<META http-equiv=""Content-Type"" content=""text/html; charset=utf-8"">
    <title>ReportServer</title>
    <link rel=""stylesheet"" href=""https://maxcdn.bootstrapcdn.com/bootstrap/3.3.7/css/bootstrap.min.css"">
    <style>
        table 
        {
            border-collapse: collapse;
            width: 80%;
        }

    th, td 
        {
            border: 1px solid Black;
            padding: 10px;
        }
    </style>
</head>
<body>"
                +
                $@"<h3 align=""center"">{tableName}</h3>"
                +
                @"<table class=""table table-bordered"">
<tr>
@foreach(var header in @Model.Headers)
{
    <th> @header </th>
}
</tr>"
                +
                groupedT
                +
                @"</table>
</body>
</html>";

            var headers = ChangeHeadersOrder(dataSet.Headers, groupColumns);

            var model = new
            {
                Headers = headers,
                Date = date
            };

            Engine.Razor = serv;
            Engine.Razor.Compile(tableTemplate, "somekey");

            return Engine.Razor.Run("somekey", null, model);
        }

        private List<string> ChangeHeadersOrder(List<string> headers, List<int> groupColumns)
        {
            int i = 0;
            foreach (var colIndex in groupColumns)
            {
                var name = headers[colIndex];
                headers.RemoveAt(colIndex);
                headers.Insert(i, name);
                i++;
            }

            return headers;
        }

        private Dictionary<MergedRow, object> GetMergedRows(List<List<object>> rows, List<int> groupCols,
            List<int> currGroupCols)
        {
            Dictionary<MergedRow, object> dict;

            var newGroupCols = currGroupCols.Skip(1).ToList();

            if (newGroupCols.Any())
            {
                dict = rows.GroupBy(row => row[currGroupCols[0]])
                    .ToDictionary(group => new MergedRow
                    {
                        Value = group.Key,
                        SpanCount = group.Count()
                    }, group => (object) group.ToList());

                for (int i = 0; i < dict.Count; i++)
                {
                    var key = dict.Keys.ElementAt(i);

                    if (dict[key] is List<List<object>> subGroup)
                    {
                        dict[key] = GetMergedRows(subGroup, groupCols, newGroupCols);
                    }
                }
            } //if this is not last column that needs to be grouped by, saving all data for further grouping

            else
            {
                dict = rows.GroupBy(row => row[currGroupCols[0]])
                    .ToDictionary(group => new MergedRow
                        {
                            Value = group.Key,
                            SpanCount = group.Count()
                        },
                        group => (object) group
                            .Select(list => list.Where((obj, index) => !groupCols.Contains(index)).ToList()).ToList());
            } //if this is last column that needs to be grouped by, save only columns that not need grouping

            return dict;
        }

        private string CreateGroupedHtmlTable(Dictionary<MergedRow, object> data)
        {
            string table = "";

            foreach (var group in data)
            {
                table += Environment.NewLine + $"<td rowspan={group.Key.SpanCount}>{group.Key.Value}</td>";

                if (group.Value is Dictionary<MergedRow, object> dict)
                    table += CreateGroupedHtmlTable(dict);
                //some tricky recursion. html shows rows correctly even if we do not use opening <tr> tag for the first row of rowspan

                else if (group.Value is List<List<object>> objs)
                {
                    foreach (var val in objs.First())
                        table += Environment.NewLine + $"<td>{val}</td>";

                    table += Environment.NewLine + "</tr>";
                    //ending of the first data row for all columns that not need grouping

                    foreach (var row in objs.Skip(1))
                    {
                        table += Environment.NewLine + "<tr>";

                        foreach (var val in row)
                            table += Environment.NewLine + $"<td>{val}</td>";

                        table += Environment.NewLine + "</tr>";

                    } //adding all other rows for all columns that not need grouping
                }
            }

            return table;
        }

        public GroupedViewExecutor(IPackageParser parser) : base(parser)
        {
        }

    } //class
}