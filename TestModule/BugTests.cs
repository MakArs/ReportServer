﻿using System;
using System.Collections.Generic;
using NUnit.Framework;
using ReportService.Core;

namespace TestModule
{
    [TestFixture]
    public class BugTests
    {
        //private readonly IArchiver archiver;

        //public BugTests(IArchiver archiver)
        //{
        //    //this.archiver = archiver;
        //}

        [Test]
        public void ArchiverDictionaryTest()
        {
            var archiver = new Archiver7Zip();

            Dictionary<string, string> someDictionary =
                new Dictionary<string, string>
                {
                    {"FirstVal", "Budsfgasgnm"},
                    {"SecondVal", "jnjnbp"}
                };

            try
            {
            var compressed = archiver.CompressString(someDictionary["safsf"]);

            }
            catch (Exception e)
            {
                var t = e;
            }

            Assert.AreEqual(1,3);
        }
    }
}