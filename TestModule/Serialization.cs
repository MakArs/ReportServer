using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Gerakul.ProtoBufSerializer;
using NUnit.Framework;
using ProtoBuf;
using ReportService.Core;

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
        public void UntypedTest()
        {
            var serializer = new ElementarySerializer();

            var encodedDescr = serializer.WriteDescriptor<FieldParams>();

            var decodedUntypedDescr = serializer.ReadDescriptor(encodedDescr);

            var ManualDescriptor = FieldParams.GetDescriptor();

            var fieldPar = new FieldParams {Number = 15, Name = "fdas", Type = "r5q"};

            var serializedField = serializer.WriteObj(fieldPar, encodedDescr);

            var manualSerializedField = ManualDescriptor.Write(fieldPar);

            var manualfield = ManualDescriptor.Read(manualSerializedField);

            using (var stream = new MemoryStream())
            {
                var header = serializer.SaveHeader<FieldParams>();

                foreach (var head in header)
                {
                    Serializer.NonGeneric.SerializeWithLengthPrefix(stream,
                        fieldPar.GetType().GetField(head.Name)?.GetValue(fieldPar),
                        PrefixStyle.Base128, head.Number);
                }

                var deserialized = new object[header.Count];

                foreach (var head in header)
                {
                    object obbj;
                    Serializer.NonGeneric
                        .TryDeserializeWithLengthPrefix(stream, PrefixStyle.Base128,
                            t => typeof(int), out obbj);

                }
            }

            var deserizalizedField = serializer.ReadObj(serializedField, encodedDescr);

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


        [Test]
        public void CopyTest()
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
                    PrefixStyle.Fixed32,
                    typeToTag[value.GetType()]);

            var tagToType = new Dictionary<int, Type>()
            {
                {1, typeof(string)},
                {2, typeof(int)},
                {3, typeof(double)},
                {4, typeof(bool)},
            };

            var expectedElements = 4;

            var readObjects = new object[] {"das", 425, 421.74, false};

            bool bul = true;

            for (int i = 0; i < expectedElements; i++)
            {
                bul = Serializer.NonGeneric.TryDeserializeWithLengthPrefix(
                    stream,
                    PrefixStyle.Base128,
                    t => tagToType[t], out readObjects[i]);
            }
        }
    }
}
