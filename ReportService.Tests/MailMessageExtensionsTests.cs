using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using NUnit.Framework;
using ReportService.Entities;
using ReportService.Extensions;
using Shouldly;

namespace ReportService.Tests
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
            message.Bcc.Select(mailAddress=> mailAddress.Address).ToList().ShouldBeEquivalentTo(expectedBccAddresses);
            message.To.Select(mailAddress=> mailAddress.Address).ShouldBeEmpty();
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
        public void ShouldThrowExceptionIfNullOperationPackage()
        {
            //Arrange
            var expectedExceptionPart = "Value cannot be null.";
            var message = new MailMessage();

            //Act, Assert
            Should.Throw<ArgumentNullException>((() => message.AddRecipientsFromPackage(null))).Message.ShouldContain(expectedExceptionPart);
        }
    }
}
