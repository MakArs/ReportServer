using System.IO;

namespace ReportService.Interfaces.Core
{
    public interface IArchiver
    {
        byte[] CompressByteArray(byte[] data);
        byte[] ExtractFromByteArchive(byte[] byteData);
        byte[] CompressStream(Stream data);
    }
}