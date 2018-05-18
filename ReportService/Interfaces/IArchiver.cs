namespace ReportService.Interfaces
{
    public interface IArchiver
    {
        byte[] CompressString(string data);
        string ExtractFromByteArchive(byte[] byteData);
    }
}
