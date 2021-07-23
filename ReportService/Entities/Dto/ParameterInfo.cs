namespace ReportService.Entities.Dto
{
    public class ParameterInfo
    {
        public string Name { get; set; }
        public string Type { get; set; }
        public bool IsRequired { get; set; }
        public string Description { get; set; }
        public string DefaultValue { get; set; }
    }
}