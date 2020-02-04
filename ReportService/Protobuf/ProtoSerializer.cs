using System;
using System.Data.Common;
using System.IO;
using Google.Protobuf.Collections;
using ProtoBuf;
using ReportService.Interfaces.Protobuf;

namespace ReportService.Protobuf
{
    public class ProtoSerializer : IProtoSerializer
    {
        public Stream WriteParametersToStream(Stream innerStream,
            RepeatedField<ColumnInfo> dataSetParameters)
        {
            return innerStream;
        }

        public Stream WriteEntityToStream(Stream innerStream, object row,
            RepeatedField<ColumnInfo> dataSetParameters)
        {
            foreach (var head in dataSetParameters)
            {
                var value = row.GetType().GetField(head.Name)?.GetValue(row);

                Serializer.NonGeneric.Serialize(innerStream, value);
            }

            return innerStream;
        }

        public Stream WriteDbReaderRowToStream(Stream innerStream, DbDataReader reader)
        {
            object[] row = new object[reader.FieldCount];

            reader.GetValues(row);

            for (int i = 0; i < row.Length; i++)
            {
                Serializer.NonGeneric.SerializeWithLengthPrefix(innerStream,
                    row[i], PrefixStyle.Base128, i + 1);
            }

            return innerStream;
        }

        public RepeatedField<ColumnInfo> ReadParametersFromStream(Stream innerStream) 
        {
            innerStream.Position = 0;

            return new RepeatedField<ColumnInfo>();
        }

        public object[] ReadRowFromStream(Stream innerStream, RepeatedField<ColumnInfo> dataSetParameters)
        {
            innerStream.Position = 0;
            var deserialized = new object[dataSetParameters.Count];

            for (int i = 0; i < deserialized.Length; i++)
            {
                Serializer.NonGeneric.Deserialize(
                    Type.GetType(dataSetParameters[i - 1].Type.ToString()), innerStream
                );
            }

            return deserialized;
        }

        public DataSet ReadDataSetFromStream(Stream innerStream)
        {
            innerStream.Position = 0; 

            DataSet set = new DataSet();

            var nextRowIndex = 0;

            while (nextRowIndex > 0)
            {
                innerStream.Position = nextRowIndex;

                var deserialized = new object[1];

                foreach (var head in set.Columns)
                {
                    Serializer.NonGeneric
                        .TryDeserializeWithLengthPrefix(innerStream, PrefixStyle.Base128,
                            t => Type.GetType(head.Type.ToString()),
                            out deserialized[0]);
                }

                //   nextRowIndex = Array.IndexOf(innerStream, new byte(), nextRowIndex) + 1; //todo:check
            }
            return set;
        }
    }
}