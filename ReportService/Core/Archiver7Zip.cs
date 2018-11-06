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

        public byte[] CompressByteArray(byte[] data)
        {
            using (var compressedStream = new MemoryStream())
            {
                using (var viewStream = new MemoryStream(data))
                {
                    compressor.CompressStream(viewStream, compressedStream);

                    return compressedStream.ToArray();
                }
            }
        }

        public byte[] ExtractFromByteArchive(byte[] byteData)
        {
            if (byteData == null || byteData.Length == 0) return null;

            using (var compressedStream = new MemoryStream(byteData))
            {
                var extractor = new SevenZipExtractor(compressedStream);

                using (var extractedStream = new MemoryStream())
                {
                    extractor.ExtractFile(0, extractedStream);
                    return extractedStream.ToArray();
                }
            }
        }

        public byte[] CompressStream(Stream data)
        {
            using (var compressedStream = new MemoryStream())
            {
                compressor.CompressStream(data, compressedStream);

                return compressedStream.ToArray();
            }
        }
    }
}