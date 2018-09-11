using System.IO;
using ReportService.Interfaces;
using SevenZip;

namespace ReportService.Core
{
    public class Archiver7Zip : IArchiver
    {
        private readonly SevenZipCompressor compressor;

        public Archiver7Zip(SevenZipCompressor compressor)
        {
            this.compressor = compressor;
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
