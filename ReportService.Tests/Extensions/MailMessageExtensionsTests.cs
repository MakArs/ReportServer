using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using NUnit.Framework;
using ReportService.Entities;
using ReportService.Extensions;
using ReportService.Protobuf;
using Shouldly;

namespace ReportService.Tests.Extensions
{
    [TestFixture]
    public class MailMessageExtensionsTests
    {
        [Test]
        public void ShouldSetAddressesFromRecipientAddresses()
        {
            //Arrange
            var expectedBccAddresses = new List<string> { "TestFooBcc@Foo.com", "TestBarBcc@Bar.Com" };
            var expectedToAddresses = new List<string> { "TestFooTo@Foo.com", "TestBarTo@Bar.Com" };

            var addresses = new RecipientAddresses
            {
                To = expectedToAddresses,
                Bcc = expectedBccAddresses
            };

            var message = new MailMessage();

            //Act
            message.AddRecipientsFromRecipientAddresses(addresses);

            //Assert
            message.To.Select(mailAddress=> mailAddress.Address).ToList().ShouldBeEquivalentTo(expectedToAddresses);
            message.Bcc.Select(mailAddress=> mailAddress.Address).ToList().ShouldBeEquivalentTo(expectedBccAddresses);
        }

        [Test]
        public void ShouldSetAddressesFromOnlyToAddresses()
        {
            //Arrange
            var expectedToAddresses = new List<string> { "TestFooTo@Foo.com", "TestBarTo@Bar.Com" };

            var addresses = new RecipientAddresses
            {
                To = expectedToAddresses
            };

            var message = new MailMessage();

            //Act
            message.AddRecipientsFromRecipientAddresses(addresses);

            //Assert
            message.To.Select(mailAddress=> mailAddress.Address).ToList().ShouldBeEquivalentTo(expectedToAddresses);
            message.Bcc.Select(mailAddress=> mailAddress.Address).ShouldBeEmpty();
        }

        [Test]
        public void ShouldSetAddressesFromOnlyBccAddresses()
        {
            //Arrange
            var expectedBccAddresses = new List<string> { "TestFooBcc@Foo.com", "TestBarBcc@Bar.Com" };

            var addresses = new RecipientAddresses
            {
                Bcc = expectedBccAddresses
            };

            var message = new MailMessage();

            //Act
            message.AddRecipientsFromRecipientAddresses(addresses);

            //Assert
            message.To.Select(mailAddress=> mailAddress.Address).ShouldBeEmpty();
            message.Bcc.Select(mailAddress=> mailAddress.Address).ToList().ShouldBeEquivalentTo(expectedBccAddresses);
        }

        [Test]
        public void ShouldThrowExceptionIfNullAddressesProvided()
        {
            //Arrange
            var expectedExceptionPart = "Value cannot be null.";
            var message = new MailMessage();

            //Act, Assert
            Should.Throw<ArgumentNullException>(() => message.AddRecipientsFromRecipientAddresses(null)).Message.ShouldContain(expectedExceptionPart);
        }

        [Test]
        public void ShouldSetAddressesFromOperationPackage()
        {
            //Arrange
            var expectedBccAddresses = new List<string> { "TestFooBcc@Foo.com", "TestBarBcc@Bar.Com" };
            var bccAddressesString = string.Join(';', expectedBccAddresses);
            var expectedToAddresses = new List<string> { "TestFooTo@Foo.com", "TestBarTo@Bar.Com" };
            var toAddressesString = string.Join(';', expectedToAddresses);

            var message = new MailMessage();
            var package = GetAddressesOperationPackage(toAddressesString, bccAddressesString);

            //Act
            message.AddRecipientsFromPackage(package);

            //Assert
            message.To.Select(mailAddress => mailAddress.Address).ToList().ShouldBeEquivalentTo(expectedToAddresses);
            message.Bcc.Select(mailAddress => mailAddress.Address).ToList().ShouldBeEquivalentTo(expectedBccAddresses);
        }

        [Test]
        public void ShouldSetAddressesFromOperationPackageWithEmptyBccAddresses()
        {
            //Arrange
            var expectedToAddresses = new List<string> { "TestFooTo@Foo.com", "TestBarTo@Bar.Com" };
            var toAddressesString = string.Join(';', expectedToAddresses);

            var message = new MailMessage();
            var package = GetAddressesOperationPackage(toAddressesString, string.Empty);

            //Act
            message.AddRecipientsFromPackage(package);

            //Assert
            message.To.Select(mailAddress=> mailAddress.Address).ToList().ShouldBeEquivalentTo(expectedToAddresses);
            message.Bcc.Select(mailAddress=> mailAddress.Address).ShouldBeEmpty();
        }

        [Test]
        public void ShouldSetAddressesFromOperationPackageWithEmptyToAddresses()
        {
            //Arrange
            var expectedBccAddresses = new List<string> { "TestFooBcc@Foo.com", "TestBarBcc@Bar.Com" };
            var bccAddressesString = string.Join(';', expectedBccAddresses);

            var message = new MailMessage();
            var package = GetAddressesOperationPackage(String.Empty, bccAddressesString);

            //Act
            message.AddRecipientsFromPackage(package);

            //Assert
            message.To.Select(mailAddress => mailAddress.Address).ShouldBeEmpty();
            message.Bcc.Select(mailAddress => mailAddress.Address).ToList().ShouldBeEquivalentTo(expectedBccAddresses);
        }
        
        [Test]
        public void ShouldSetAddressesFromOperationPackageWithEmptyAddresses()
        {
            //Arrange
            var message = new MailMessage();
            var package = GetAddressesOperationPackage(string.Empty, string.Empty);

            //Act
            message.AddRecipientsFromPackage(package);

            //Assert
            message.Bcc.Select(mailAddress => mailAddress.Address).ToList().ShouldBeEmpty();
            message.To.Select(mailAddress => mailAddress.Address).ShouldBeEmpty();
        }
        
        [Test]
        public void ShouldSetAddressesFromOperationPackageWithoutAddresses()
        {
            //Arrange
            var message = new MailMessage();
            var package = GetAddressesOperationPackage(string.Empty, string.Empty);

            //Act
            message.AddRecipientsFromPackage(package);

            //Assert
            message.Bcc.Select(mailAddress => mailAddress.Address).ToList().ShouldBeEmpty();
            message.To.Select(mailAddress => mailAddress.Address).ShouldBeEmpty();
        }

        [Test]
        public void ShouldThrowExceptionIfNullOperationPackage()
        {
            //Arrange
            var expectedExceptionPart = "Value cannot be null.";
            var message = new MailMessage();

            //Act, Assert
            Should.Throw<ArgumentNullException>((() => message.AddRecipientsFromPackage(null))).Message.ShouldContain(expectedExceptionPart);
        }

        private OperationPackage GetAddressesOperationPackage(string toAddresses, string bccAddresses)
        {
            var set = new[]
            {
                new
                {
                    Address = toAddresses,
                    RecType = "To"
                },
                new
                {
                    Address = bccAddresses,
                    RecType = "Bcc"
                }
            };

            var packageBuilder = new ProtoPackageBuilder();
            var package = packageBuilder.GetPackage(set);
            return package;
        }

        private OperationPackage GetEmptyOperationPackage()
        {
            var set = new[] { new{} };

            var packageBuilder = new ProtoPackageBuilder();
            var package = packageBuilder.GetPackage(set);
            return package;
        }
    }
}
