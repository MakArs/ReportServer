using System.Collections.Generic;
using NUnit.Framework;
using ReportService.Entities;
using Shouldly;

namespace ReportService.Tests.Entities
{
    [TestFixture]
    public class RecipientGroupTests
    {
        [Test]
        public void ShouldReturnAddresses_GivenToAndBccAddressesSet()
        {
            //Arrange
            var expectedToAddressesList = new List<string> { "Foo@foo.to", "Bar@bar.to" };
            var expectedBccAddressesList = new List<string> { "Bar@foo.bcc", "Foo@bar.bcc" };

            var group = new RecipientGroup
            {
                AddressesBcc = string.Join(';', expectedBccAddressesList),
                Addresses = string.Join(';', expectedToAddressesList)
            };

            //Act
            RecipientAddresses addresses = group.GetAddresses();

            //Assert
            addresses.To.ShouldBeEquivalentTo(expectedToAddressesList);
            addresses.Bcc.ShouldBeEquivalentTo(expectedBccAddressesList);
        }

        [Test]
        public void ShouldReturnAddresses_GivenToAddressesSet()
        {
            //Arrange
            var expectedToAddressesList = new List<string> { "Foo@foo.to", "Bar@bar.to" };

            var group = new RecipientGroup
            {
                Addresses = string.Join(';', expectedToAddressesList)
            };

            //Act
            RecipientAddresses addresses = group.GetAddresses();

            //Assert
            addresses.To.ShouldBeEquivalentTo(expectedToAddressesList);
            addresses.Bcc.ShouldBeNull();
        }

        [Test]
        public void ShouldReturnOnlyValidAddresses_GivenToAddressesSet()
        {
            //Arrange
            var givenToAddressesList = new List<string> { "Foo@foo.to", "@InvalidAddress", "Bar@bar.to", "InvalidAddress" };
            var expectedToAddressesList = new List<string> { "Foo@foo.to", "Bar@bar.to" };

            var group = new RecipientGroup
            {
                Addresses = string.Join(';', givenToAddressesList)
            };

            //Act
            RecipientAddresses addresses = group.GetAddresses();

            //Assert
            addresses.To.ShouldBeEquivalentTo(expectedToAddressesList);
            addresses.Bcc.ShouldBeNull();
        }

        [Test]
        public void ShouldReturnAddresses_GivenBccAddressesSet()//todo ArsMak: Check if message can be sent without To section filled (most probably is not case). If not - add error during parsing (RecipientGroup?)
        {
            //Arrange
            var expectedBccAddressesList = new List<string> { "Bar@foo.bcc", "Foo@bar.bcc" };

            var group = new RecipientGroup
            {
                AddressesBcc = string.Join(';', expectedBccAddressesList)
            };

            //Act
            RecipientAddresses addresses = group.GetAddresses();

            //Assert
            addresses.To.ShouldBeNull();
            addresses.Bcc.ShouldBeEquivalentTo(expectedBccAddressesList);
        }
    }
}
