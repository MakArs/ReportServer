namespace ReportService.Interfaces
{
    public interface IArchiver
    {
        byte[] CompressString(string data);
        string ExtractFromBytes(byte[] byteData);
    }
}
