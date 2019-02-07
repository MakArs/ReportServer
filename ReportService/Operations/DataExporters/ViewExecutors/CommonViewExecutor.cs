using System;
using System.Data;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using CsvHelper;
using OfficeOpenXml;
using RazorEngine;
using RazorEngine.Configuration;
using RazorEngine.Templating;
using ReportService.Extensions;
using ReportService.Interfaces.Core;
using ReportService.Interfaces.Protobuf;

namespace ReportService.Operations.DataExporters.ViewExecutors
{
    public class CommonViewExecutor : IViewExecutor
    {
        private readonly IPackageBuilder packageBuilder;

        public CommonViewExecutor(IPackageBuilder builder)
        {
            packageBuilder = builder;
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

            var packageValues = packageBuilder.GetPackageValues(package);

            var model = new
            {
                packageValues.First().Headers,
                Content = packageValues.First().Rows,
                Date = date
            };

            return Engine.Razor.Run("somekey", null, model);
        }

        public virtual string ExecuteTelegramView(OperationPackage package,
            string reportName = "Отчёт")
        {
            var packageValues = packageBuilder.GetPackageValues(package);

            var tmRep = $@"*{reportName}*";

            foreach (var dataset in packageValues)
            {
                tmRep = tmRep.Insert(tmRep.Length,
                    Environment.NewLine + Environment.NewLine + $"_{dataset.Name}_");

                tmRep = tmRep.Insert(tmRep.Length, Environment.NewLine +
                                                   string.Join(" | ",
                                                       dataset.Headers.Select(head =>
                                                           Regex.Replace(head, @"([[*_])",
                                                               @"\$1"))));

                foreach (var row in dataset.Rows)
                    tmRep = tmRep.Insert(tmRep.Length, Environment.NewLine + string.Join(" | ", row
                                                           .Select(val =>
                                                               val is string strVal
                                                                   ? Regex.Replace(strVal,
                                                                       @"([[*_])", @"\$1")
                                                                   : val)));
            }

            return tmRep;
        }

        public ExcelPackage ExecuteXlsx(OperationPackage package, string reportName)
        {
            var pack = new ExcelPackage();
            var ws = pack.Workbook.Worksheets.Add(reportName);

            var firstSet = packageBuilder.GetPackageValues(package).First();

            var propNum = 0;

            foreach (string header in firstSet.Headers)
                ws.Cells[1, ++propNum].Value = header;

            using (ExcelRange rng = ws.Cells[1, 1, 1, propNum])
            {
                rng.Style.Font.Bold = true;
                rng.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                rng.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGray);
            }

            int i = 0;

            foreach (var row in firstSet.Rows)
            {
                i++;
                int j = 0;

                foreach (var value in row)
                {
                    j++;
                    ws.Cells[i + 1, j].SetObjValue(value, "");
                }
            }

            ws.Cells[1, 1, i, propNum].AutoFitColumns();
            return pack;
        }

        public byte[] ExecuteCsv(OperationPackage package, string delimiter = ";") //byte[] because closing inner streams causes closing of external one
        {
            var csvStream = new MemoryStream();
            try
            {

                var firstSet = packageBuilder.GetPackageValues(package).First();

                using (var writerStream = new StreamWriter(csvStream))
                {
                    using (var csvWriter = new CsvWriter(writerStream))
                    {
                        csvWriter.Configuration.Delimiter = delimiter;
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
