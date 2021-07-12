using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ReportService.Api.Controllers
{
    [ApiController]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
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

        protected ContentResult GetSuccessfulResult(string content)
        {
            return new ContentResult
            {
                Content = content,
                StatusCode = StatusCodes.Status200OK
            };
        }

        protected ContentResult GetNotFoundErrorResult(string content)
        {
            return new ContentResult
            {
                Content = content,
                StatusCode = StatusCodes.Status404NotFound
            };
        }

        protected ContentResult GetBadRequestError(string content)
        {
            return new ContentResult
            {
                Content = content,
                StatusCode = StatusCodes.Status400BadRequest
            };
        }
    }
}