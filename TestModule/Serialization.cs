using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Google.Protobuf;
using NUnit.Framework;
using ProtoBuf;
using ReportService;
using ReportService.Entities;
using ReportService.Entities.Dto;
using ReportService.Protobuf;
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
            var taskContext = new Dictionary<string, object>
            {
                {"@table", "table3"},
                {"@par", "id"},
                {"@value", 15}
            };

            var Query = "Select * from @table where @par=@value";

            var parameters = new List<object>();
            Regex regex = new Regex(@"\@\w+\b");

            var selections = regex.Matches(Query);

            List<object> values = new List<object>();

            for (int i = 0; i < selections.Count; i++)
            {
                var sel = selections[i].Value;
                var paramValue = taskContext[sel];
                values.Add(paramValue);
                Query=Query.Replace(sel, $@"@p{i}");
            }

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

            var readObjects = new object[] {"124", 25, 04.24, "ds"};

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


        public void ExampleProtoUsing()
        {
            OperationPackage package = new OperationPackage();
            var t = package.DataSets;
            ColumnInfo info = new ColumnInfo();

            TestingProtoInt protoint = new TestingProtoInt
            {
                ColumnCount = {1, 1, 1, 1, 1, 1, 1, 1, 32112}
            };

            TestingProtoInt2 protoInt2 = new TestingProtoInt2
            {
                ColumnCount = {1, 1, 1, 1, 1, 1, 1, 1, 32112},
                //RowCount = {2, 2, 2, 2, 2, 2, 2, 2}
            };

            using (var output =
                File.OpenWrite(@"C:\ArsMak\ReportServer\TestModule\exampleserialized.dat"))
            {
                protoint.WriteTo(output);
            }

            using (var output =
                File.OpenWrite(@"C:\ArsMak\ReportServer\TestModule\exampleint2-d.dat"))
            {
                protoint.WriteDelimitedTo(output);
                protoint.WriteDelimitedTo(output);
            }

            using (var output =
                File.OpenWrite(@"C:\ArsMak\ReportServer\TestModule\exampleint2-nd.dat"))
            {
                protoint.WriteTo(output);
                protoint.WriteTo(output);
            }

        }

        [Test]
        public void ProtoBuilderTest()
        {
            var list=new List<int>{1,3,5,2154,125,2567,976,3};
            var asd = list.Take(4);

            var е = new Random().Next(100);
            var offs = new DateTime(1980, 3, 4, 21, 45, 52);

            var unix = ((DateTimeOffset)offs).ToUnixTimeSeconds();

            var back = DateTimeOffset
                .FromUnixTimeSeconds(unix);

            var backutc= DateTimeOffset
                .FromUnixTimeSeconds(unix).UtcDateTime;

            var opers = new List<DtoOperation>
            {
                new DtoOperation
                {
                    Config = "gds",
                    Id = 13,
                    ImplementationType = null,
                    IsDefault = true,
                    IsDeleted = true,
                    Name = "hnerwe",
                    Number = 1,
                    TaskId = 32
                },
                new DtoOperation
                {
                    Config = "gd9-s",
                    Id = 1334,
                    ImplementationType = "9098",
                    Name = "hnerwe",
                    Number = 51,
                    TaskId = 232
                },
            };

            var bldr = new ProtoPackageBuilder();

            var packfrm = bldr.GetPackage(opers);

            var prsr=new ProtoPackageParser();

            var deser = prsr.GetPackageValues(packfrm);
        }
    }
}
