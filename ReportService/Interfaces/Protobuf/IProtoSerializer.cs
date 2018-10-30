using System.Data.Common;
using System.IO;
using ReportService.Core;

namespace ReportService.Interfaces.Protobuf
{
    public interface IProtoSerializer
    {
        Stream AddEntityToStream(Stream innerStream, object row, DataSetDescriptor descriptor);

        Stream AddDbReaderRowToStream(Stream innerStream, DbDataReader reader,
                              DataSetDescriptor descriptor);

        DataSetRow ReadRowFromByteArray(Stream innerStream, DataSetDescriptor descriptor);

        DataSet ReadDataSetFromByteArray(byte[] innerStream);
    }
}
