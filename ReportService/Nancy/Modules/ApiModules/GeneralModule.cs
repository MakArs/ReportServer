using System.Linq;
using Nancy;
using Nancy.Swagger.Annotations.Attributes;
using Newtonsoft.Json;
using Swagger.ObjectModel;
using Response = Nancy.Response;

namespace ReportService.Nancy.Modules.ApiModules
{
    public sealed class GeneralModule : NancyBaseModule
    {
        public const string IsAliveRoute = "/api/v2";
        public const string GetRoleRoute = "/api/v2/roles";

        [Route(nameof(IsAlive))]
        [Route(HttpMethod.Get, IsAliveRoute)]
        [Route(
            Tags = new[] {"General"},
            Summary = "Method for checking if service is alive")]
        [SwaggerResponse(HttpStatusCode.OK, Message = "Success")]
        [SwaggerResponse(
            HttpStatusCode.InternalServerError,
            "Internal error during request execution")]
        public Response IsAlive()
        {
            try
            {
                return HttpStatusCode.OK;
            }
            catch
            {
                return HttpStatusCode.InternalServerError;
            }
        }

        [Route(nameof(GetUserRole))]
        [Route(HttpMethod.Get, GetRoleRoute)]
        [Route(
            Tags = new[] {"General"},
            Summary = "Method for getting role of request user")]
        [SwaggerResponse(HttpStatusCode.OK, Message = "Success")]
        [SwaggerResponse(
            HttpStatusCode.InternalServerError,
            "Internal error during request execution")]
        public Response GetUserRole()
        {
            try
            {
                var claims = Context.CurrentUser.Claims
                    .FirstOrDefault(claim => claim.Type == PermissionsType)?.Value;

                ApiUserRole role = ApiUserRole.NoRole;

                if (claims != null)
                    if (claims.Contains(EditPermission))
                        role = ApiUserRole.Editor;
                    else if (claims.Contains(StopRunPermission))
                        role = ApiUserRole.StopRunner;
                    else if (claims.Contains(ViewPermission))
                        role = ApiUserRole.Viewer;

                var response = (Response) JsonConvert.SerializeObject(role);
                response.StatusCode = HttpStatusCode.OK;
                return response;
            }
            catch
            {
                return HttpStatusCode.InternalServerError;
            }
        }

        public GeneralModule()
        {
            Get(IsAliveRoute,
                _ => IsAlive(),
                name: nameof(IsAlive));

            Get(GetRoleRoute, _ =>
                    GetUserRole(),
                name: nameof(GetUserRole));

        }
    }
}