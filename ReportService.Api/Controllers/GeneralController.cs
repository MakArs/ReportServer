using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Domain0.Tokens;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
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
        public GeneralController(IConfigurationRoot config) : base(config) { }


        [HttpGet]
        public IActionResult IsAlive()
        {
            try
            {
                return StatusCode(200);
            }
            catch
            {
                return StatusCode(500);
            }
        }

        [Authorize(Domain0Auth.Policy)]
        [HttpGet(GetUserRoleRoute)]
        public ContentResult GetUserRole()
        {
            if (!User.Identity.IsAuthenticated)
                return new ContentResult
                {
                    Content = "Please, authenticate in domain0",
                    StatusCode = StatusCodes.Status401Unauthorized
                };

            try
            {
                var claims = User.Claims
                    .FirstOrDefault(claim => claim.Type == "permissions")?.Value;

                ApiUserRole role = ApiUserRole.NoRole;

                if (claims != null)
                    if (claims.Contains(EditPermission))
                        role = ApiUserRole.Editor;
                    else if (claims.Contains(StopRunPermission))
                        role = ApiUserRole.StopRunner;
                    else if (claims.Contains(ViewPermission))
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