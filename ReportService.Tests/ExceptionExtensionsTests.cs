using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using ReportService.Extensions;
using Shouldly;

namespace ReportService.Tests
{
    [TestFixture]
    public class ExceptionExtensionsTests
    {
        [Test]
        public void ShouldParseSingleErrorProperly()
        {
            //Arrange
            var expectedExceptionMessage = "FooMessage";
            var exception = new Exception(expectedExceptionMessage);

            //Act
            IEnumerable<Exception> exceptionTree = exception.GetExceptionTree();

            //Assert
            exceptionTree.Single().Message.ShouldBe(expectedExceptionMessage);
        }

        [Test]
        public void ShouldParseComplexExceptionProperly()
        {
            //Arrange
            var expectedExceptionMessage = "FooMessage";
            var expectedInnerExceptionMessage = "BarMessage";
            var expectedExceptionsCount = 2;
            var innerException = new ApplicationException(expectedInnerExceptionMessage);
            var exception = new InvalidOperationException(expectedExceptionMessage, innerException);

            //Act
            List<Exception> exceptionTree = exception.GetExceptionTree().ToList();

            //Assert
            exceptionTree.Count.ShouldBe(expectedExceptionsCount);
            exceptionTree.First().ShouldBe(exception);
            exceptionTree.Last().ShouldBe(innerException);

            List<string> exceptionMessagesTree = exceptionTree.Select(ex => ex.Message).ToList();
            exceptionMessagesTree.ShouldContain(expectedExceptionMessage);
            exceptionMessagesTree.ShouldContain(expectedInnerExceptionMessage);
        }
    }
}
