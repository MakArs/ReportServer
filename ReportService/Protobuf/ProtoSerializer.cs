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
            //add byte [header]
            return innerStream;
        }

        public Stream WriteEntityToStream(Stream innerStream, object row,
                                          RepeatedField<ColumnInfo> dataSetParameters)
        {
            //add byte[row]

            foreach (var head in dataSetParameters)
            {
                var value = row.GetType().GetField(head.Name)?.GetValue(row);

                Serializer.NonGeneric.Serialize(innerStream,value);

                //Serializer.NonGeneric.SerializeWithLengthPrefix(innerStream,
                //    value, PrefixStyle.Base128, head.Key);
            }

            return innerStream;
        }

        public Stream WriteDbReaderRowToStream(Stream innerStream, DbDataReader reader)
        {
            //add byte[row]

            object[] row=new object[reader.FieldCount];

            reader.GetValues(row);

            for (int i = 0; i < row.Length; i++)
            {
                Serializer.NonGeneric.SerializeWithLengthPrefix(innerStream,
                    row[i], PrefixStyle.Base128, i+1);
            }

            return innerStream;
        }

        public RepeatedField<ColumnInfo> ReadParametersFromStream(Stream innerStream) //tested without byte-separator
        {
            //read byte[header]

            innerStream.Position = 0;
            
            return new RepeatedField<ColumnInfo>();
        }

        public object[] ReadRowFromStream(Stream innerStream, RepeatedField<ColumnInfo> dataSetParameters)
        {
            //read byte[row]
            innerStream.Position = 0;
            var deserialized = new object[dataSetParameters.Count];

            //foreach (var head in dataSetParameters.Fields)
            //{
            //    Serializer.NonGeneric
            //        .TryDeserializeWithLengthPrefix(innerStream, PrefixStyle.Base128,
            //            t => Type.GetType(head.Value.TypeName),
            //            out deserialized[head.Key - 1]);
            //}

            for (int i = 0; i < deserialized.Length; i++)
            {
                Serializer.NonGeneric.Deserialize(
                    Type.GetType(dataSetParameters[i - 1].Type.ToString()),innerStream
                );
            }

            return deserialized;
        }

        public DataSet ReadDataSetFromStream(Stream innerStream)
        {
            //read byte[row]

            innerStream.Position = 0;//specific bytes..

            DataSet set=new DataSet();

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

             //   nextRowIndex = Array.IndexOf(innerStream, new byte(), nextRowIndex) + 1;
            }


            return set;
        }
    }
}