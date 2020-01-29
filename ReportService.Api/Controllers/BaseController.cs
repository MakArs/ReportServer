using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

namespace ReportService.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BaseController : ControllerBase
    {
        protected string PermissionsType = "permissions";
        protected string ViewPermission;
        protected string EditPermission;
        protected string StopRunPermission;
        protected BaseController(IConfigurationRoot config)
        {
            ViewPermission = config["PermissionsSettings:Permissions_View"];
            EditPermission = config["PermissionsSettings:Permissions_Edit"];
            StopRunPermission = config["PermissionsSettings:Permissions_StopRun"];
        }
    }
}