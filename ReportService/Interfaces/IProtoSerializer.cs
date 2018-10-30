using System.Collections.Generic;
using System.IO;

namespace ReportService.Interfaces
{
    public interface IProtoSerializer
    {
        Stream AddRow(Stream innerStream, object row, Dictionary<int, ColumnInfo> descriptor);

        object ReadRow(Stream innerStream, Dictionary<int, ColumnInfo> descriptor);
        List<object> ReadDataSets(byte[] innerStream);
    }
}
