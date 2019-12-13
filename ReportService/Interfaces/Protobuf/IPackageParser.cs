using System.Collections.Generic;
using ReportService.Entities;

namespace ReportService.Interfaces.Protobuf
{
    public interface IPackageParser
    {
        List<DataSetContent> GetPackageValues(OperationPackage package);
    }
}