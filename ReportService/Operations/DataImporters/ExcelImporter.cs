using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using AutoMapper;
using Newtonsoft.Json;
using OfficeOpenXml;
using ReportService.Interfaces.Core;
using ReportService.Interfaces.ReportTask;

namespace ReportService.Operations.DataImporters
{
    public class ExcelImporter : IDataImporter
    {
        public int Id { get; set; }
        public bool IsDefault { get; set; }
        public int Number { get; set; }
        public string Name { get; set; }
        public string DataSetName { get; set; }
        public string FilePath;
        public string SheetName;
        public bool SkipEmptyRows;
        public string[] ColumnList;
        public bool UseColumnNames;
        public int FirstDataRow;
        public int MaxRowCount;

        public ExcelImporter(IMapper mapper, ExcelImporterConfig config)
        {
            mapper.Map(config, this);
        }

        public void Execute(IRTaskRunContext taskContext)
        {
            var fi = new FileInfo(FilePath);
            var queryResult = new List<Dictionary<string, string>>();

            using (var pack = new ExcelPackage(fi))
            {
                var sheet = string.IsNullOrEmpty(SheetName)
                    ? pack.Workbook.Worksheets.First()
                    : pack.Workbook.Worksheets.First(workSheet => workSheet.Name == SheetName);


                var names = new string[ColumnList.Length];

                int firstValueRow;

                int lastValueRow =
                    Math.Min(sheet.Cells.Last(cell => !string.IsNullOrEmpty(cell.Text)).End.Row,
                        MaxRowCount);

                if (UseColumnNames)
                {
                    firstValueRow = FirstDataRow + 1;
                    for (int i = 0; i < ColumnList.Length; i++)
                        names[i] = sheet.Cells[$"{ColumnList[i]}{FirstDataRow}"]
                            .Text;
                }

                else
                {
                    firstValueRow = FirstDataRow;
                    names = ColumnList;
                }

                for (int i = firstValueRow; i <= lastValueRow; i++)
                {
                    var fields = new Dictionary<string, string>();
                    for (int j = 0; j < ColumnList.Length; j++)
                        fields
                            .Add(names[j], sheet.Cells[$"{ColumnList[j]}{i}"].Text);

                    if (SkipEmptyRows && fields.All(field => string.IsNullOrEmpty(field.Value)))
                        continue;

                    queryResult.Add(fields);
                }
            }

            var jsString = JsonConvert.SerializeObject(queryResult);
            taskContext.DataSets[DataSetName] = jsString;
        }
    }
}