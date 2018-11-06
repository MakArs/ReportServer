﻿using System.IO;
using AutoMapper;
using OfficeOpenXml;
using ReportService.Interfaces.Core;
using ReportService.Interfaces.Protobuf;
using ReportService.Interfaces.ReportTask;

namespace ReportService.Operations.DataImporters
{
    public class ExcelPackageReadingParameters
    {
        public string SheetName;
        public bool SkipEmptyRows;
        public string[] ColumnList;
        public bool UseColumnNames;
        public int FirstDataRow;
        public int MaxRowCount;
    }

    public class ExcelImporter : IDataImporter
    {
        private readonly IPackageBuilder packageBuilder;
        public ExcelPackageReadingParameters ExcelParameters;

        public int Id { get; set; }
        public bool IsDefault { get; set; }
        public int Number { get; set; }
        public string Name { get; set; }
        public string PackageName { get; set; }
        public string FilePath;

        public ExcelImporter(IMapper mapper, ExcelImporterConfig config, IPackageBuilder builder)
        {
            mapper.Map(config, this);
            ExcelParameters = new ExcelPackageReadingParameters();
            mapper.Map(config, ExcelParameters);
            packageBuilder = builder;
        }

        public void Execute(IRTaskRunContext taskContext)
        {
            var fi = new FileInfo(FilePath);

            using (var pack = new ExcelPackage(fi))
            {
                var package = packageBuilder.GetPackage(pack, ExcelParameters);
                taskContext.Packages[PackageName] = package;
            }
        }
    }
}