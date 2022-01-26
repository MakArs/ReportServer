namespace ReportService.Entities.Dto
{
    public class DateRangeValidationRule : ValidationRule
    {
        public EnumValidationSettingsId SettingsId { get; set; }
        public string LinkedParameterName { get; set; }
        public int MaxDays { get; set; }
    }
}
