namespace ReportService.Interfaces
{
    public interface IDataImporter
    {
        string DataSetName { get; set; }
        string Execute();
    }

    public interface IImporterConfig
    {
        string DataSetName { get; set; }
    }
}
