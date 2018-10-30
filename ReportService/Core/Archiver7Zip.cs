using System;
using System.IO;
using System.Reflection;
using ReportService.Interfaces.Core;
using SevenZip;

namespace ReportService.Core
{
    public class Archiver7Zip : IArchiver
    {
        private readonly SevenZipCompressor compressor;

        public Archiver7Zip()
        {
            var path = Path.Combine(
                Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ??
                throw new InvalidOperationException(),
                Environment.Is64BitProcess ? "x64" : "x86", "7z.dll");

            SevenZipBase.SetLibraryPath(path);

            compressor = new SevenZipCompressor
            {
                CompressionMode = CompressionMode.Create,
                ArchiveFormat = OutArchiveFormat.SevenZip
            };
        }

        public byte[] CompressString(string data)
        {
            using (var compressedStream = new MemoryStream())
            {
                var dataBytes = System.Text.Encoding.UTF8.GetBytes(data);

                using (var viewStream = new MemoryStream(dataBytes))
                {
                    compressor.CompressStream(viewStream, compressedStream);

                    return compressedStream.ToArray();
                }
            }
        }

        public string ExtractFromByteArchive(byte[] byteData)
        {
            if (byteData == null || byteData.Length == 0) return null;

            using (var compressedStream = new MemoryStream(byteData))
            {
                var extractor = new SevenZipExtractor(compressedStream);

                using (var extractedStream = new MemoryStream())
                {
                    extractor.ExtractFile(0, extractedStream);

                    return System.Text.Encoding.UTF8.GetString(extractedStream.ToArray());
                }
            }
        }
    }
}