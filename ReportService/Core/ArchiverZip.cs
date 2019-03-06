using System;
using System.IO;
using ICSharpCode.SharpZipLib.Core;
using ICSharpCode.SharpZipLib.Zip;
using ReportService.Interfaces.Core;

namespace ReportService.Core
{
    public class ArchiverZip : IArchiver
    {
        public byte[] CompressByteArray(byte[] byteData)
        {
            using (var stream = new MemoryStream(byteData))
            {
                return CompressStream(stream);
            }
        }

        public byte[] ExtractFromByteArchive(byte[] byteData)
        {
            if (byteData == null || byteData.Length == 0) return null;

            using (var compressedStream = new MemoryStream(byteData))
            {
                using (ZipInputStream zipInputStream = new ZipInputStream(compressedStream))
                {
                    using (var extractedStream = new MemoryStream())
                    {
                        byte[] buffer = new byte[4096];
                        zipInputStream.GetNextEntry();
                        StreamUtils.Copy(zipInputStream, extractedStream, buffer);
                        compressedStream.Close(); // Must finish the ZipOutputStream before using outputMemStream.

                        return extractedStream.ToArray();
                    }
                }
            }
        }

        public byte[] CompressStream(Stream data)
        {
            using (MemoryStream outputMemStream = new MemoryStream())
            {
                using (ZipOutputStream zipStream = new ZipOutputStream(outputMemStream))
                {
                    zipStream.SetLevel(3); //0-9, 9 being the highest level of compression


                    ZipEntry newEntry = new ZipEntry("OperationPackage")
                        {DateTime = DateTime.Now};

                    zipStream.PutNextEntry(newEntry);
                    data.Position = 0;
                    StreamUtils.Copy(data, zipStream, new byte[4096]);
                    zipStream.CloseEntry();

                    zipStream.IsStreamOwner = false; // False stops the Close also Closing the underlying stream.
                    zipStream.Close(); // Must finish the ZipOutputStream before using outputMemStream.

                    outputMemStream.Position = 0;

                    byte[] byteArrayOut = outputMemStream.ToArray();

                    return byteArrayOut;
                }
            }
        }
    }
}