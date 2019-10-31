namespace ReportService.Entities.ServiceSettings
{
    public class TokenValidationSettings
    {
        public string Token_Audience { get; set; }
        public string Token_Issuer { get; set; }
        public string Token_Alg { get; set; }
        public string Token_Secret { get; set; }
    }
}