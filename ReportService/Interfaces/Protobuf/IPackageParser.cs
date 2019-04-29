using System.Collections.Generic;
using ReportService.Protobuf;

namespace ReportService.Interfaces.Protobuf
{
    public interface IPackageParser
    {
        List<DataSetContent> GetPackageValues(OperationPackage package);
    }
}