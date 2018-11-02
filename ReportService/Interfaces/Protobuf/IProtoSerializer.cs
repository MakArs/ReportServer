using System.Data.Common;
using System.IO;
using ReportService.Protobuf;

namespace ReportService.Interfaces.Protobuf
{
    public interface IProtoSerializer
    {
        Stream WriteParametersToStream(Stream innerStream, DataSetParameters dataSetParameters);

        Stream WriteEntityToStream(Stream innerStream, object row, DataSetParameters dataSetParameters);

        Stream WriteDbReaderRowToStream(Stream innerStream, DbDataReader reader);

        DataSetParameters ReadParametersFromStream(Stream innerStream);

        object[] ReadRowFromStream(Stream innerStream, DataSetParameters dataSetParameters);

        DataSet ReadDataSetFromStream(Stream innerStream);
    }
}
