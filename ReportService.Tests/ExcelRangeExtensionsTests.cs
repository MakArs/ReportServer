using System;
using System.Linq;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using OfficeOpenXml;
using ReportService.Extensions;
using Shouldly;

namespace ReportService.Tests
{
    [TestFixture]
    public class ExcelRangeExtensionsTests
    {
        [Test]
        [TestCase(123, 123, TestName = "IntValue")]
        [TestCase(1.23, 1.23, TestName = "DoubleValue")]
        [TestCase(9223372036854775807, 9223372036854775807, TestName = "LongValue")]
        [TestCase(true, true, TestName = "BoolValue")]
        [TestCase("FooBar", "FooBar", TestName = "StringValue")]
        public void ShouldSetValue(object value, object expectedValue)
        {
            //Arrange
            ExcelRange excelRange = GetExcelRangeForTest();

            //Act
            excelRange.SetFromObject(value);

            //Assert
            excelRange.Value.ShouldBe(expectedValue);
        }

        [Test]
        public void ShouldSetValueFromShort()
        {
            //Arrange
            short expectedValue = 123;
            ExcelRange excelRange = GetExcelRangeForTest();

            //Act
            excelRange.SetFromObject(expectedValue);

            //Assert
            excelRange.Value.ShouldBe(expectedValue);
        }

        [Test]
        public void ShouldSetValueFromByte()
        {
            //Arrange
            byte expectedValue = 123;
            ExcelRange excelRange = GetExcelRangeForTest();

            //Act
            excelRange.SetFromObject(expectedValue);

            //Assert
            excelRange.Value.ShouldBe(expectedValue);
        }

        [Test]
        public void ShouldSetValueFromDecimal()
        {
            //Arrange
            decimal expectedValue = 1.235623623768237636239623626m;
            ExcelRange excelRange = GetExcelRangeForTest();

            //Act
            excelRange.SetFromObject(expectedValue);

            //Assert
            excelRange.Value.ShouldBe(expectedValue);
        }

        [Test]
        public void ShouldSetValueFromDateTimeWithUnsetDateTime()
        {
            //Arrange
            DateTime expectedValue = new DateTime(2121, 3, 1);
            ExcelRange excelRange = GetExcelRangeForTest();

            //Act
            excelRange.SetFromObject(expectedValue);

            //Assert
            excelRange.Value.ShouldBe(expectedValue);
            excelRange.Style.Numberformat.Format.ShouldBe("dd.mm.yyyy");
        }

        [Test]
        public void ShouldSetValueFromDateTimeWithSetDateTime()
        {
            //Arrange
            DateTime expectedValue = new DateTime(2121, 3, 1, 12, 34, 56);
            ExcelRange excelRange = GetExcelRangeForTest();

            //Act
            excelRange.SetFromObject(expectedValue);

            //Assert
            excelRange.Value.ShouldBe(expectedValue);
            excelRange.Style.Numberformat.Format.ShouldBe("dd.mm.yyyy HH:mm:ss");
        }

        [Test]
        public void ShouldSetValueFromClass()
        {
            //Arrange
            var value = new DummyClass();
            var expectedValue = "ReportService.Tests.ExcelRangeExtensionsTests+DummyClass";
            ExcelRange excelRange = GetExcelRangeForTest();

            //Act
            excelRange.SetFromObject(value);

            //Assert
            excelRange.Value.ShouldBe(expectedValue);
        }
        
        private ExcelRange GetExcelRangeForTest()
        {
            var package = new ExcelPackage();
            package.Workbook.Worksheets.Add("Test");
            return package.Workbook.Worksheets.First().Cells[1, 1];
        }

        private class DummyClass {}
    }
}
