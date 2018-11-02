using System;
using System.Collections.Generic;
using System.IO;
using Google.Protobuf;
using NUnit.Framework;
using ProtoBuf;
using ReportService;
using ReportService.Interfaces.Core;
using ReportService.Protobuf;
using ElementarySerializer = ReportService.Protobuf.ElementarySerializer;

namespace TestModule
{
    [TestFixture]
    public class SerializerTester
    {
        [Test]
        public void GenericTest()
        {
            var serializer = new ElementarySerializer();

            var encodedDescr = serializer.WriteDescriptor<FieldParams>();

            var decodedTypedDesct = serializer.ReadDescriptor<FieldParams>(encodedDescr);

            var ManualDescriptor = FieldParams.GetDescriptor();

            var fieldPar = new FieldParams {Number = 15, Name = "fdas", Type = "r5q"};

            var serializedField = serializer.WriteObj(fieldPar, encodedDescr);

            var manualSerializedField = ManualDescriptor.Write(fieldPar);

            Assert.That(serializedField,
                Is.EqualTo(manualSerializedField));

            var manualfield = ManualDescriptor.Read(manualSerializedField);

            var deserizalizedField = decodedTypedDesct.Read(serializedField);
        }

        [Test]
        public void CopiedUntypedTest()
        {
            var data = new object[]
            {
                "Hello",
                42,
                0.3529321,
                true
            };

            var typeToTag = new Dictionary<Type, int>()
            {
                {typeof(string), 1},
                {typeof(int), 2},
                {typeof(double), 3},
                {typeof(bool), 4},
            };

            var stream = new MemoryStream();

            foreach (var value in data)
                Serializer.NonGeneric.SerializeWithLengthPrefix(
                    stream,
                    value,
                    PrefixStyle.Base128,
                    typeToTag[value.GetType()]);

            stream.Position = 0;

            var tagToType = new Dictionary<int, Type>()
            {
                {1, typeof(string)},
                {2, typeof(int)},
                {3, typeof(double)},
                {4, typeof(bool)},
            };

            var expectedElements = 4;

            var readObjects = new object[] { "124", 25, 04.24, "ds" };

            bool bul = true;

            for (int i = 0; i < expectedElements; i++)
            {
                bul = Serializer.NonGeneric.TryDeserializeWithLengthPrefix(
                    stream,
                    PrefixStyle.Base128,
                    t => tagToType[t], out readObjects[i]);
            }

            for (int i = 0; i < expectedElements; i++)
            {
                Assert.AreEqual(data[i], readObjects[i]);
            }
        }

        [Test]
        public void UntypedArrayTest()
        {
            var serializer = new ElementarySerializer();

            var fieldPar = new FieldParams {Number = 15, Name = "fdas", Type = "r5q"};
           
            using (var stream = new MemoryStream())
            {
                var header = serializer.SaveHeaderFromClassFields<FieldParams>();

                var varAsArray = new List<object>();

                foreach (var head in header.Fields)
                {
                    var value = fieldPar.GetType().GetField(head.Value.Name)?.GetValue(fieldPar);

                    Serializer.NonGeneric.SerializeWithLengthPrefix(stream,
                        value, PrefixStyle.Base128, head.Key);

                    varAsArray.Add(value);
                }

                stream.Position = 0;

                var deserialized = new object[header.Fields.Count];

                foreach (var head in header.Fields)
                {
                    Serializer.NonGeneric
                        .TryDeserializeWithLengthPrefix(stream, PrefixStyle.Base128,
                            t => Type.GetType(head.Value.TypeName),
                            out deserialized[head.Key-1]);
                }

                for (int i = 0; i< varAsArray.Count; i++)
                {
                Assert.AreEqual(varAsArray[i], deserialized[i]);
                }
            }

            var testOper = new DtoOperation
            {
                Id=14,
                TaskId = 163,
                Number = 3,
                Name = "testOperr",
                ImplementationType = "NoImplemented",
                IsDeleted = false,
                IsDefault = true,
                Config = "{}"
            };
            
            using (var stream = new MemoryStream())
            {
                var header = serializer.SaveHeaderFromClassFields<DtoOperation>();

                var varAsArray = new List<object>();

                foreach (var head in header.Fields)
                {
                    var value = testOper.GetType().GetField(head.Value.Name)?.GetValue(testOper);

                    Serializer.NonGeneric.SerializeWithLengthPrefix(stream,
                        value, PrefixStyle.Base128, head.Key);

                    varAsArray.Add(value);
                }

                stream.Position = 0;

                var deserialized = new object[header.Fields.Count];

                foreach (var head in header.Fields)
                {
                    Serializer.NonGeneric
                        .TryDeserializeWithLengthPrefix(stream, PrefixStyle.Base128,
                            t => Type.GetType(head.Value.TypeName),
                            out deserialized[head.Key - 1]);
                }

                for (int i = 0; i < varAsArray.Count; i++)
                {
                    Assert.AreEqual(varAsArray[i], deserialized[i]);
                }
            }
        }

        [Test]
        public void ProtoSerializerTest()
        {
            var testOper = new DtoOperation
            {
                Id = 14,
                TaskId = 163,
                Number = 3,
                Name = "testOperr",
                ImplementationType = "NoImplemented",
                IsDeleted = false,
                IsDefault = true,
                Config = "{}"
            };

            var descrbuilder=new DescriptorBuilder();

            var info = descrbuilder.GetClassParameters<DtoOperation>();

            var serializer=new ProtoSerializer();

            var stream=new MemoryStream();
            var stream2 = new MemoryStream();

            serializer.WriteParametersToStream(stream,info);

            serializer.WriteEntityToStream(stream2, testOper,
                serializer.ReadParametersFromStream(stream));

            var newinfo = serializer.ReadParametersFromStream(stream);
            var newFields = serializer.ReadRowFromStream(stream,newinfo);
        }

        [Test]
        public void ExampleProtoUsing()
        {
            //TestingProtoMessage proto =new TestingProtoMessage
            //{
            //    ColumnCount = 0,
            //    Columns = { new ColumnInfo
            //    {
            //        Tag = { 1,5,2,5},
            //        Name = { "412","vxzw","g215d"},
            //        TypeName = "Int32Type"
            //    },
            //        new ColumnInfo{
            //            Tag = {521,321,332,755},
            //            Name = { "41gsdf2","vxzb cvw",@"g43-\215d"},
            //            TypeName = "Intg32Type"
            //        }}
            //};

            TestingProtoInt protoint=new TestingProtoInt
            {
                ColumnCount = { 1,1,1,1,1,1,1,1,32112}
            };

            TestingProtoInt2 protoInt2 = new TestingProtoInt2
            {
                ColumnCount = {1, 1, 1, 1, 1, 1, 1, 1, 32112 },
                //RowCount = {2, 2, 2, 2, 2, 2, 2, 2}
            };

            using (var output = File.OpenWrite(@"C:\ArsMak\ReportServer\TestModule\exampleserialized.dat"))
            {
                protoint.WriteTo(output);
            }

            using (var output=File.OpenWrite(@"C:\ArsMak\ReportServer\TestModule\exampleint2-d.dat"))
            {
                protoint.WriteDelimitedTo(output);
                protoint.WriteDelimitedTo(output);
            }

            using (var output = File.OpenWrite(@"C:\ArsMak\ReportServer\TestModule\exampleint2-nd.dat"))
            {
                protoint.WriteTo(output);
                protoint.WriteTo(output);
            }
        }
    }
}
