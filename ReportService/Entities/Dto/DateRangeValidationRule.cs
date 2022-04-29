namespace ReportService.Entities.Dto
{
    public class DateRangeValidationRule : ValidationRule
    {
        public string ValidationRuleName { get; set; }
        public string LinkedParameterName { get; set; }
        public int MaxDays { get; set; }
    }
}
