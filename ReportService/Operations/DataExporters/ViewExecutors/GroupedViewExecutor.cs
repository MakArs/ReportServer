using OfficeOpenXml;
using ReportService.Extensions;
using ReportService.Interfaces.Protobuf;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using OfficeOpenXml.Style;
using RazorLight;
using ReportService.Entities;

namespace ReportService.Operations.DataExporters.ViewExecutors
{
    public class GroupedViewExecutor : CommonViewExecutor
    {
        public GroupedViewExecutor(IPackageParser parser) : base(parser)
        {
        }

        public override string ExecuteHtml(string tableName, OperationPackage package)
        {
            string date = $"{DateTime.Now:dd.MM.yy HH:mm:ss}";


            if (!package.DataSets.Any()) return "No information obtained by query";

            var packageValues = PackageParser.GetPackageValues(package);

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

            var engine = new RazorLightEngineBuilder()
                .UseEmbeddedResourcesProject(typeof(Program))
                .UseMemoryCachingProvider()
                .Build();

            var result = engine.CompileRenderStringAsync("templateKey", tableTemplate, model).Result;

            return result;
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

        protected override void AddDataSetToExcel(ExcelPackage inPackage, DataSetContent content)
        {
            var ws = inPackage.Workbook.Worksheets.Add(
                string.IsNullOrEmpty(content.Name) ? "NoNamedList" : content.Name);
            var propNum = 0;


            var groupColumns = content.GroupColumns;

            var grouping = GetMergedRows(content.Rows, groupColumns, groupColumns);
            var headers = ChangeHeadersOrder(content.Headers, groupColumns);

            foreach (string header in headers)
                ws.Cells[1, ++propNum].Value = header;

            using (ExcelRange rng = ws.Cells[1, 1, 1, propNum])
            {
                rng.Style.Font.Bold = true;
                rng.Style.Fill.PatternType = ExcelFillStyle.Solid;
                rng.Style.Fill.BackgroundColor.SetColor(Color.LightGray);
            }

            var sizes = new Dictionary<int, double>();

            FillGroup(ws, grouping, 1, 2, sizes);

            var i = grouping.Keys.Sum(key => key.SpanCount);

            ws.Cells[1, 1, i, propNum].AutoFitColumns(5, 50);

            for (int j = 1; j <= propNum; j++)
            {
                ws.Column(j).Style.WrapText = true;
            }

            for (int j = 1; j <= sizes.Count; j++)
                ws.Column(j).Width = sizes[j];
        }

        private void FillGroup(ExcelWorksheet ws, Dictionary<MergedRow, object> grouping, int column, int startRow,
            Dictionary<int, double> columnSizes)
        {
            foreach (var group in grouping)
            {
                var span = group.Key.SpanCount;

                ws.Cells[startRow, column].SetObjValue(group.Key.Value, "");
                ws.Cells[startRow, startRow + span].Merge = true;

                if (group.Value is Dictionary<MergedRow, object> dict)
                    FillGroup(ws, dict, column + 1, startRow, columnSizes);

                else if (group.Value is List<List<object>> objs)
                {
                    int nonGroupedStartRow = startRow;

                    foreach (var row in objs)
                    {
                        int j = column;

                        foreach (var value in row)
                        {
                            j++;
                            ws.Cells[nonGroupedStartRow, j].SetObjValue(value, "");
                        }

                        nonGroupedStartRow++;
                    }
                }

                ws.Cells[startRow, column, startRow, column].AutoFitColumns(5, 50);

                var newSize = ws.Column(column).Width;
                if (columnSizes.ContainsKey(column))
                {
                    var currentSize = columnSizes[column];

                    if (newSize > currentSize)
                        columnSizes[column] = newSize;
                }

                else
                    columnSizes.Add(column, newSize);

                ws.Cells[startRow, column, startRow + span - 1, column].Merge = true;

                startRow += span;
            }
        }

        public override ExcelPackage ExecuteXlsx(OperationPackage package, string reportName, bool useAllSets = false)
        {
            var pack = new ExcelPackage();

            var packageContent = PackageParser.GetPackageValues(package);

            if (useAllSets)
            {
                for (int i = 0; i < packageContent.Count; i++)
                {
                    if (packageContent[i].GroupColumns != null)
                        AddDataSetToExcel(pack, packageContent[i]);
                    else
                        base.AddDataSetToExcel(pack, packageContent[i]);

                    if (pack.Workbook.Worksheets[i].Name == "NoNamedList")
                        pack.Workbook.Worksheets[i].Name = $"Dataset{i + 1}";
                }
            }

            else AddDataSetToExcel(pack, packageContent.First());

            return pack;
        }
    } //class
}