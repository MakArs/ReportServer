using System.Linq;
using Domain0.Tokens;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace ReportService.Api.Controllers
{
    [Route("api/v2")]
    [ApiController]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public class GeneralController : BaseController
    {
        private const string GetUserRoleRoute = "roles";
        
        [HttpGet]
        public IActionResult IsAlive()
        {
            try
            {
                return StatusCode(200);
            }
            catch
            {
                return GetInternalErrorResult();
            }
        }

        [Authorize(Domain0Auth.Policy)]
        [HttpGet(GetUserRoleRoute)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public ContentResult GetUserRole()
        {
            try
            {
                var claims = User.Claims
                    .FirstOrDefault(claim => claim.Type == PermissionsType)?.Value;

                ApiUserRole role = ApiUserRole.NoRole;

                if (claims != null)
                    if (claims.Contains("reporting.edit"))
                        role = ApiUserRole.Editor;
                    else if (claims.Contains("reporting.stoprun"))
                        role = ApiUserRole.StopRunner;
                    else if (claims.Contains("reporting.view"))
                        role = ApiUserRole.Viewer;

                var response = new ContentResult
                {
                    Content = JsonConvert.SerializeObject(role),
                    StatusCode = StatusCodes.Status200OK,
                    ContentType = "text/html"
                };

                return response;
            }
            catch
            {
                return new ContentResult
                {
                    Content = "Internal error during request execution",
                    StatusCode = StatusCodes.Status500InternalServerError
                };
            }
        }
    }
}