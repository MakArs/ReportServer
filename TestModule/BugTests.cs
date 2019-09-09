using System;
using System.Collections.Generic;
using System.Text;
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
            var compressed = archiver.CompressByteArray(Encoding.UTF8.GetBytes(
                someDictionary["safsf"]));

            }
            catch (Exception e)
            {
                var t = e;
            }

        }
    }
}
