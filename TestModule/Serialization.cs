using System;
using System.Collections.Generic;
using System.IO;
using Google.Protobuf;
using NUnit.Framework;
using ProtoBuf;
using ReportService;
using ElementarySerializer = TestModule.ProtoOld.ElementarySerializer;
using FieldParams = TestModule.ProtoOld.FieldParams;

namespace TestModule
{
    [TestFixture]
    public class SerializerTester
    {
        [Test]
        public void GenericTest()
        {
            (string country, string capital, double gdpPerCapita) das =
                ("Malawi", "Lilongwe", 226.50);

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

            OperationPackage package = new OperationPackage();
            var t = package.DataSets;
            ColumnInfo info=new ColumnInfo();

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
