using System.Data.Common;
using System.IO;
using Google.Protobuf.Collections;

namespace ReportService.Interfaces.Protobuf
{
    public interface IProtoSerializer
    {
        Stream WriteParametersToStream(Stream innerStream, RepeatedField<ColumnInfo> dataSetParameters);

        Stream WriteEntityToStream(Stream innerStream, object row, RepeatedField<ColumnInfo> dataSetParameters);

        Stream WriteDbReaderRowToStream(Stream innerStream, DbDataReader reader);

        RepeatedField<ColumnInfo> ReadParametersFromStream(Stream innerStream);

        object[] ReadRowFromStream(Stream innerStream, RepeatedField<ColumnInfo> dataSetParameters);

        DataSet ReadDataSetFromStream(Stream innerStream);
    }
}
