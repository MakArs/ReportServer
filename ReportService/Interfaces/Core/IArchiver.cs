namespace ReportService.Interfaces.Core
{
    public interface IArchiver
    {
        byte[] CompressString(string data);
        string ExtractFromByteArchive(byte[] byteData);
    }
}
