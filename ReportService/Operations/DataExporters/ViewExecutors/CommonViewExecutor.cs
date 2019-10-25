using CsvHelper;
using OfficeOpenXml;
using RazorEngine;
using RazorEngine.Configuration;
using RazorEngine.Templating;
using ReportService.Extensions;
using ReportService.Interfaces.Core;
using ReportService.Interfaces.Protobuf;
using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using ReportService.Entities;

namespace ReportService.Operations.DataExporters.ViewExecutors
{
    public class CommonViewExecutor : IViewExecutor
    {
        protected readonly IPackageParser PackageParser;

        public CommonViewExecutor(IPackageParser parser)
        {
            PackageParser = parser;
        }

        public virtual string ExecuteHtml(string viewTemplate, OperationPackage package)
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

            if (!package.DataSets.Any()) return "No information obtained by query";

            var packageValues = PackageParser.GetPackageValues(package);

            var dataSet = packageValues.First();

            var model = new
            {
                dataSet.Headers,
                Content = dataSet.Rows,
                Date = date
            };

            return Engine.Razor.Run("somekey", null, model);
        }

        private string AddDataSetToTelegView(string tmRep, DataSetContent content)
        {
            tmRep = tmRep.Insert(tmRep.Length,
                Environment.NewLine + Environment.NewLine + $"_{content.Name}_");

            tmRep = tmRep.Insert(tmRep.Length, Environment.NewLine +
                                               string.Join(" | ",
                                                   content.Headers.Select(head =>
                                                       Regex.Replace(head, @"([[*_])",
                                                           @"\$1"))));

            foreach (var row in content.Rows)
                tmRep = tmRep.Insert(tmRep.Length, Environment.NewLine + string.Join(" | ", row
                                                       .Select(val =>
                                                           val is string strVal
                                                               ? Regex.Replace(strVal,
                                                                   @"([[*_])", @"\$1")
                                                               : val)));

            return tmRep;
        }


        public virtual string ExecuteTelegramView(OperationPackage package,
            string reportName = "Отчёт", bool useAllSets = false)
        {
            var packageValues = PackageParser.GetPackageValues(package);

            var tmRep = $@"*{reportName}*";

            if (useAllSets)
            {
                foreach (var dataset in packageValues)
                    tmRep = AddDataSetToTelegView(tmRep, dataset);
            }

            else
                tmRep = AddDataSetToTelegView(tmRep, packageValues.First());

            return tmRep;
        }

        protected virtual void AddDataSetToExcel(ExcelPackage inPackage, DataSetContent content)
        {
            var ws = inPackage.Workbook.Worksheets.Add(
                string.IsNullOrEmpty(content.Name) ? "NoNamedList" : content.Name);
            var propNum = 0;

            foreach (string header in content.Headers)
                ws.Cells[1, ++propNum].Value = header;

            using (ExcelRange rng = ws.Cells[1, 1, 1, propNum])
            {
                rng.Style.Font.Bold = true;
                rng.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                rng.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGray);
            }

            int i = 0;

            foreach (var row in content.Rows)
            {
                i++;
                int j = 0;

                foreach (var value in row)
                {
                    j++;
                    ws.Cells[i + 1, j].SetObjValue(value, "");
                }
            }

            ws.Cells[1, 1, i, propNum].AutoFitColumns(5, 50);

            for (int j = 1; j <= propNum; j++)
            {
                ws.Column(j).Style.WrapText = true;
            }

            // return inPackage;
        }

        public virtual ExcelPackage ExecuteXlsx(OperationPackage package, string reportName, bool useAllSets = false)
        {
            var pack = new ExcelPackage();

            var packageContent = PackageParser.GetPackageValues(package);

            if (useAllSets)
            {
                for (int i = 0; i < packageContent.Count; i++)
                {
                    AddDataSetToExcel(pack, packageContent[i]);
                    if (pack.Workbook.Worksheets[i + 1].Name == "NoNamedList")
                        pack.Workbook.Worksheets[i + 1].Name = $"Dataset{i + 1}";
                }

                // foreach (var set in packageContent)
            }

            else AddDataSetToExcel(pack, packageContent.First());

            return pack;
        }

        public byte[] ExecuteCsv(OperationPackage package,
            string delimiter = ";",
            bool useAllSets = false) //byte[] because closing inner streams causes closing of external one
        {
            var csvStream = new MemoryStream();
            try
            {
                using (var writerStream = new StreamWriter(csvStream))
                {
                    using (var csvWriter = new CsvWriter(writerStream))
                    {
                        csvWriter.Configuration.Delimiter = delimiter;

                        if (!useAllSets)
                        {
                            var firstSet = PackageParser.GetPackageValues(package).First();
                            foreach (var head in firstSet.Headers)
                            {
                                csvWriter.WriteField(head);
                            }

                            csvWriter.NextRecord();

                            foreach (var row in firstSet.Rows)
                            {
                                foreach (var value in row)
                                    csvWriter.WriteField(value);

                                csvWriter.NextRecord();
                            }
                        }

                        else
                        {
                            foreach (var dataSet in PackageParser.GetPackageValues(package))
                            {
                                foreach (var head in dataSet.Headers)
                                {
                                    csvWriter.WriteField(head);
                                }

                                csvWriter.NextRecord();

                                foreach (var row in dataSet.Rows)
                                {
                                    foreach (var value in row)
                                        csvWriter.WriteField(value);

                                    csvWriter.NextRecord();
                                }
                            }
                        }

                        writerStream.Flush();
                        csvStream.Position = 0;
                    }
                }

                return csvStream.ToArray();
            }

            finally
            {
                csvStream.Dispose();
            }
        }
    } //class
}