﻿using ReportService.Interfaces.Protobuf;

namespace ReportService.Operations.DataExporters.ViewExecutors
{
    public class CommonTableViewExecutor : CommonViewExecutor
    {
        public override string ExecuteHtml(string tableName, OperationPackage package)
        {
            string tableTemplate = @"<!DOCTYPE html>
<html>
<head>
<META http-equiv=""Content-Type"" content=""text/html; charset=utf-8"">
    <title>ReportServer</title>
    <link rel=""stylesheet"" href=""https://maxcdn.bootstrapcdn.com/bootstrap/3.3.7/css/bootstrap.min.css"">
    <style>
        table {
            border-collapse: collapse;
            width: 80%;
        }

    th, td {
            border: 1px solid Black;
            padding: 10px;
        }
    </style>
</head>
<body>"
                                   +
                                   $@"<h3 align=""center"">{tableName}</h3>"
                                   +
                                   @"<table class=""table table-bordered table-hover "">
<tr>
@foreach(var header in @Model.Headers)
{
<th> @header </th>
}
</tr>
        @foreach(var props in @Model.Content)
{
        <tr>
            @foreach(var prop in @props)
            {
             <td> @prop</td>
            }
        </tr>
        }
    </table>
</body>
</html>";
            return base.ExecuteHtml(tableTemplate, package);
        }

        public CommonTableViewExecutor(IPackageParser parser) : base(parser)
        {
        }
    } //class
}