using System.Linq;
using Domain0.Tokens;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;

namespace ReportService.Api.Controllers
{
    [Route("api/v3")]
    [ApiController]
    public class GeneralController : BaseController
    {
        private const string GetUserRoleRoute = "roles";
        private readonly IConfigurationRoot configurationRoot;

        public GeneralController(IConfigurationRoot configurationRoot)
        {
            this.configurationRoot = configurationRoot;
        }

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
                    if (claims.Contains(configurationRoot["PermissionsSettings:Permissions_Edit"]))
                        role = ApiUserRole.Editor;
                    else if (claims.Contains(configurationRoot["PermissionsSettings:Permissions_StopRun"]))
                        role = ApiUserRole.StopRunner;
                    else if (claims.Contains(configurationRoot["PermissionsSettings: Permissions_View"]))
                        role = ApiUserRole.Viewer;

                return GetSuccessfulResult(JsonConvert.SerializeObject(role));
            }
            catch
            {
                return GetInternalErrorResult();
            }
        }
    }
}