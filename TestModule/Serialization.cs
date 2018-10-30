using System;
using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using ProtoBuf;
using ReportService.Core;
using ReportService.Interfaces;

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

            //var encodedDescr = serializer.WriteDescriptor<FieldParams>();

            //var decodedUntypedDescr = serializer.ReadDescriptor(encodedDescr);

            //var ManualDescriptor = FieldParams.GetDescriptor();


            //var serializedField = serializer.WriteObj(fieldPar, encodedDescr);

            //var manualSerializedField = ManualDescriptor.Write(fieldPar);

            //var manualfield = ManualDescriptor.Read(manualSerializedField);

            //var deserizalizedField = serializer.ReadObj(serializedField, encodedDescr);

            //var someEntityList = new List<SomeEntity>
            //{
            //    new SomeEntity
            //    {
            //        IntField = 25,
            //        DoubleField = 2342.125512,
            //        StringField = "FirstEntity"
            //    },
            //    new SomeEntity {IntField = 12, DoubleField = 0.03421, StringField = "SecondEntity"},
            //    new SomeEntity {IntField = 893, DoubleField = 90, StringField = "ThirdEntity"}
            //};

            //var buff = serializer.WriteList(someEntityList, smEntDscr);

            //var listAgain = serializer.GetEntities(buff, smEntDscr);

            //Assert.That(listAgain.SequenceEqual(someEntityList));
        }

    }
}
