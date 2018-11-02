using System;
using System.Data.Common;
using System.IO;
using Gerakul.ProtoBufSerializer;
using ProtoBuf;
using ReportService.Interfaces.Protobuf;

namespace ReportService.Protobuf
{
    public class ProtoSerializer : IProtoSerializer
    {
        private readonly MessageDescriptor<DataSetParameters> descriptor;

        public ProtoSerializer()
        {
            descriptor = DataSetParameters.GetDescriptor();
        }

        public Stream WriteParametersToStream(Stream innerStream,
                                              DataSetParameters dataSetParameters)
        {
            //add byte [header]

            var writer = descriptor.CreateWriter(innerStream);
            writer.Write(dataSetParameters);
            writer.Close();
            return innerStream;
        }

        public Stream WriteEntityToStream(Stream innerStream, object row,
                                          DataSetParameters dataSetParameters)
        {
            //add byte[row]

            foreach (var head in dataSetParameters.Fields)
            {
                var value = row.GetType().GetField(head.Value.Name)?.GetValue(row);

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

        public DataSetParameters ReadParametersFromStream(Stream innerStream) //tested without byte-separator
        {
            //read byte[header]

            innerStream.Position = 0;

            var reader = descriptor.CreateReader(innerStream);

            var dsParams = reader.Read();

            reader.Close();

            return dsParams;
        }

        public object[] ReadRowFromStream(Stream innerStream, DataSetParameters dataSetParameters)
        {
            //read byte[row]
            innerStream.Position = 0;
            var deserialized = new object[dataSetParameters.Fields.Count];

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
                    Type.GetType(dataSetParameters.Fields[i - 1].TypeName),innerStream
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

                var deserialized = new object[set.dataSetParameters.Fields.Count];

                foreach (var head in set.dataSetParameters.Fields)
                {
                    Serializer.NonGeneric
                        .TryDeserializeWithLengthPrefix(innerStream, PrefixStyle.Base128,
                            t => Type.GetType(head.Value.TypeName),
                            out deserialized[head.Key - 1]);
                }

                set.Rows.Add(deserialized);

             //   nextRowIndex = Array.IndexOf(innerStream, new byte(), nextRowIndex) + 1;
            }


            return set;
        }
    }
}