using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ReportService.Api.Controllers
{
    [ApiController]
    public class BaseController : ControllerBase
    {
        protected readonly string PermissionsType = "permissions";
        private readonly string InternalErrorMessage = "Internal error during request execution";
        protected ContentResult GetInternalErrorResult()
        {
            return new ContentResult
            {
                Content = InternalErrorMessage,
                StatusCode = StatusCodes.Status500InternalServerError
            };
        }
    }
}