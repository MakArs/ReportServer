using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using OfficeOpenXml;
using ReportService.Interfaces;

namespace ReportService.DataImporters
{
    public class ExcelImporter : IDataImporter
    {
        public int Id { get; set; }
        public int Number { get; set; }
        public string DataSetName { get; set; }
        private readonly string filePath;
        private readonly string sheetName;
        private readonly bool skipEmptyRows;
        private readonly string[] columnList;
        private readonly bool useColumnNames;
        private readonly int firstDataRow;
        private readonly int maxRowCount;

        public ExcelImporter(string jsonConfig)
        {
            var excelConfig = JsonConvert
                .DeserializeObject<ExcelImporterConfig>(jsonConfig);

            Number = excelConfig.Number;
            DataSetName = excelConfig.DataSetName;
            filePath = excelConfig.FilePath;
            sheetName = excelConfig.ScheetName;
            skipEmptyRows = excelConfig.SkipEmptyRows;
            columnList = excelConfig.ColumnList;
            useColumnNames = excelConfig.UseColumnNames;
            firstDataRow = excelConfig.FirstDataRow;
            maxRowCount = excelConfig.MaxRowCount;
        }

        public string Execute()
        {
            var fi = new FileInfo(filePath);
            var queryResult = new List<Dictionary<string, string>>();

            using (var pack = new ExcelPackage(fi))
            {
                var sheet = string.IsNullOrEmpty(sheetName)
                    ? pack.Workbook.Worksheets.First()
                    : pack.Workbook.Worksheets.First(workSheet => workSheet.Name == sheetName);


                var names = new string[columnList.Length];

                int firstValueRow;

                int lastValueRow =
                    Math.Min(sheet.Cells.Last(cell => !string.IsNullOrEmpty(cell.Text)).End.Row,
                        maxRowCount);

                if (useColumnNames)
                {
                    firstValueRow = firstDataRow + 1;
                    for (int i = 0; i < columnList.Length; i++)
                        names[i] = sheet.Cells[$"{columnList[i]}{firstDataRow}"]
                            .Text;
                }

                else
                {
                    firstValueRow = firstDataRow;
                    names = columnList;
                }

                for (int i = firstValueRow; i <= lastValueRow; i++)
                {
                    var fields = new Dictionary<string, string>();
                    for (int j = 0; j < columnList.Length; j++)
                        fields
                            .Add(names[j], sheet.Cells[$"{columnList[j]}{i}"].Text);

                    if (skipEmptyRows && fields.All(field => string.IsNullOrEmpty(field.Value)))
                        continue;

                        queryResult.Add(fields);
                }
            }

            return JsonConvert.SerializeObject(queryResult);
        }
    }
}
