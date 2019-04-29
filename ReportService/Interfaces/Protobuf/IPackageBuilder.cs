using System.Collections.Generic;
using System.Data.Common;
using CsvHelper;
using OfficeOpenXml;
using ReportService.Operations.DataImporters;

namespace ReportService.Interfaces.Protobuf
{
    public interface IPackageBuilder
    {
        OperationPackage GetPackage(DbDataReader reader);

        OperationPackage GetPackage(CsvReader reader);

        OperationPackage GetPackage<T>(IEnumerable<T> values) where T : class;

        OperationPackage GetPackage(ExcelPackage excelPackage,
            ExcelPackageReadingParameters excelParameters);
    }
}