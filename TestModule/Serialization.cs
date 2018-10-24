using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using ReportService.Core;
using ReportService.Interfaces;

namespace TestModule
{
    [TestFixture]
    public class UnitTest1
    {
        [Test]
        public void TestMethod1()
        {
            //    var flds= typeof(FieldParams).GetFields();
            //    var type = flds.First().FieldType.Name;
            var serializer = new ElementarySerializer();
            var codedDescr = serializer.WriteDescriptor<FieldParams>();
            var decodedDescr = serializer.ReadDescriptor<FieldParams>(codedDescr);
            var smEntDscr = FieldParams.GetDescriptor();

            var fieldPar = new FieldParams { Number = 15,Name = "fdas",Type = "r5q"};

            Assert.That(decodedDescr.Write(fieldPar),
                Is.EqualTo(smEntDscr.Write(fieldPar)));

            var someEntityList=new List<SomeEntity>
            {
                new SomeEntity{IntField = 25,DoubleField = 2342.125512,StringField = "FirstEntity"},
                new SomeEntity{IntField = 12,DoubleField = 0.03421,StringField = "SecondEntity"},
                new SomeEntity{IntField = 893,DoubleField = 90,StringField = "ThirdEntity"}
            };

            //var buff = serializer.WriteList(someEntityList, smEntDscr);

            //var listAgain = serializer.GetEntities(buff, smEntDscr);

            //Assert.That(listAgain.SequenceEqual(someEntityList));
        }
    }

}
