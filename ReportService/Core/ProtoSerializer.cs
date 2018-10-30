using System;
using System.Data.Common;
using System.IO;
using Gerakul.ProtoBufSerializer;
using ProtoBuf;
using ReportService.Interfaces.Protobuf;

namespace ReportService.Core
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
            byte[] serializedParams = descriptor.Write(dataSetParameters);
            innerStream.Write(serializedParams, 0, serializedParams.Length);
            return innerStream;
        }

        public Stream WriteEntityToStream(Stream innerStream, object row,
                                          DataSetParameters dataSetParameters)
        {
            foreach (var head in dataSetParameters.Fields)
            {
                var value = row.GetType().GetField(head.Value.Name)?.GetValue(row);

                Serializer.NonGeneric.SerializeWithLengthPrefix(innerStream,
                    value, PrefixStyle.Base128, head.Key);
            }

            return innerStream;
        }

        public Stream WriteDbReaderRowToStream(Stream innerStream, DbDataReader reader)
        {
            object[] row=new object[reader.FieldCount];

            reader.GetValues(row);

            for (int i = 0; i < row.Length; i++)
            {
                Serializer.NonGeneric.SerializeWithLengthPrefix(innerStream,
                    row[i], PrefixStyle.Base128, i+1);
            }

            return innerStream;
        }

        public DataSetParameters ReadDescriptorFromByteArray(byte[] innerStream)
        {
           // var str=new MemoryStream();
           // str.Write(innerStream,0,innerStream.Length);
            return descriptor.Read(innerStream);
        }

        public object[] ReadRowFromByteArray(byte[] innerStream, DataSetParameters dataSetParameters)
        {
            var str = new MemoryStream();

            str.Write(innerStream,0,innerStream.Length);

            str.Position = Array.IndexOf(innerStream, new byte()) + 1;

            var deserialized = new object[dataSetParameters.Fields.Count];

            foreach (var head in dataSetParameters.Fields)
            {
                Serializer.NonGeneric
                    .TryDeserializeWithLengthPrefix(str, PrefixStyle.Base128,
                        t => Type.GetType(head.Value.TypeName),
                        out deserialized[head.Key - 1]);
            }

            str.Dispose();

            return deserialized;
        }

        public DataSet ReadDataSetFromByteArray(byte[] innerStream)
        {
            var set = new DataSet
            {
                dataSetParameters = ReadDescriptorFromByteArray(innerStream)
            };

            var str = new MemoryStream();
            str.Write(innerStream, 0, innerStream.Length);

            str.Position = Array.IndexOf(innerStream, new byte());//specific bytes..

            var nextRowIndex= Array.IndexOf(innerStream, new byte())+1;

            while (nextRowIndex > 0)
            {
                str.Position = nextRowIndex;

                var deserialized = new object[set.dataSetParameters.Fields.Count];

                foreach (var head in set.dataSetParameters.Fields)
                {
                    Serializer.NonGeneric
                        .TryDeserializeWithLengthPrefix(str, PrefixStyle.Base128,
                            t => Type.GetType(head.Value.TypeName),
                            out deserialized[head.Key - 1]);
                }

                set.Rows.Add(deserialized);

                nextRowIndex = Array.IndexOf(innerStream, new byte(), nextRowIndex) + 1;
            }

            str.Dispose();

            return set;
        }
    }
}