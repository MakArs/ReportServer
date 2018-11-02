using System.Collections.Generic;
using System.Data.Common;
using OfficeOpenXml;

namespace ReportService.Interfaces.Protobuf
{
    public interface IPackageBuilder
    {
        OperationPackage GetPackage(DbDataReader reader);

        OperationPackage GetPackage<T>(IEnumerable<T> values) where T:class;

        OperationPackage GetPackage(ExcelPackage excelPackage);
    }
   
}