using System.Collections.Generic;
using System.Data.Common;
using CsvHelper;
using OfficeOpenXml;
using ReportService.Operations.DataImporters;

namespace ReportService.Interfaces.Protobuf
{
    public interface IPackageBuilder
    {
        OperationPackage GetPackage(DbDataReader reader, string groupNumbers);

        OperationPackage GetPackage(CsvReader reader, string groupNumbers);

        OperationPackage GetPackage<T>(IEnumerable<T> values) where T : class;

        OperationPackage GetPackage(ExcelPackage excelPackage,
            ExcelPackageReadingParameters excelParameters, string groupNumbers);
    }
}