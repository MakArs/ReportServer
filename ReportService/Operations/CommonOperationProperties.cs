namespace ReportService.Operations
{
    public class CommonOperationProperties
    {
        public int Id { get; set; }
        public bool IsDefault { get; set; }
        public bool NeedSavePackage { get; set; }
        public int Number { get; set; }
        public string Name { get; set; }
        public string PackageName { get; set; }
    }
}
