using System;
using System.IO;
using ReportService.Interfaces;

namespace ReportService.Implementations
{
    class PostMasterTest : IPostMaster
    {
        private string filename;

        public void Send(string report)
        {
            filename = $"Report{DateTime.Now.ToString("HHmmss")}.html";

            using (FileStream fs = new FileStream($@"C:\ArsMak\job\{filename}", FileMode.CreateNew))
            { byte[] array = System.Text.Encoding.Default.GetBytes(@"<!DOCTYPE html>
                < html >
                < head >
                < title > Page Title </ title >
                </ head >
                < body >

                < h1 > This is a Heading </ h1 >
                < p > This is a paragraph.</ p >

                </ body >
                </ html > ");
                fs.Write(array, 0, array.Length);
            }

            Console.WriteLine($"file {filename} saved to disk...");

        }
    }
}
