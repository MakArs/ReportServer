using System.Data.Common;
using System.IO;
using ReportService.Core;

namespace ReportService.Interfaces.Protobuf
{
    public interface IProtoSerializer
    {
        Stream WriteParametersToStream(Stream innerStream, DataSetParameters dataSetParameters);

        Stream WriteEntityToStream(Stream innerStream, object row, DataSetParameters dataSetParameters);

        Stream WriteDbReaderRowToStream(Stream innerStream, DbDataReader reader);

        DataSetParameters ReadDescriptorFromByteArray(byte[] innerStream);

        object[] ReadRowFromByteArray(byte[] innerStream, DataSetParameters dataSetParameters);

        DataSet ReadDataSetFromByteArray(byte[] innerStream);
    }
}
