using Microsoft.AspNetCore.Mvc.Testing;

namespace ReportService.Tests
{
    namespace App.API.Tests.Integration
    {
        public class APIWebApplicationFactory : WebApplicationFactory<ReportService.Api.Startup>
        {
        }
    }
}