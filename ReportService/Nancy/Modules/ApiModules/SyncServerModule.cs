using Nancy;
using Nancy.ModelBinding;
using Nancy.Security;
using Nancy.Swagger.Annotations.Attributes;
using ReportService.Entities.ServiceSettings;
using ReportService.Interfaces.Core;
using ReportService.Nancy.Models;
using Swagger.ObjectModel;
using Response = Nancy.Response;

namespace ReportService.Nancy.Modules.ApiModules
{
    public sealed class SyncServerModule : NancyBaseModule
    {
        public const string PostTaskTemplateRoute = "/api/v2/synchronizations/tasks";

        [Route(nameof(CreateTaskByTemplate))]
        [Route(HttpMethod.Post, PostTaskTemplateRoute)]
        [Route(Consumes = new[] {"application/json"})]
        [Route(
            Tags = new[] {"Sync server"},
            Summary = "Method for creating task through syncserver")]
        [RouteParam(
            ParamIn = ParameterIn.Body,
            Name = "task template",
            ParamType = typeof(ApiTask),
            Required = true,
            Description = "template of task with some defined parameters")]
        [RouteParam(
            ParamIn = ParameterIn.Header,
            Name = "Authorization",
            ParamType = typeof(string),
            Required = true,
            Description = "JWT access token")]
        [SwaggerResponse(
            HttpStatusCode.OK,
            Message = "Success",
            Model = typeof(long))]
        [SwaggerResponse(
            HttpStatusCode.InternalServerError,
            "Internal error during request execution")]
        public Response CreateTaskByTemplate(ILogic logic)
        {
            this.RequiresClaims(c => c.Type == PermissionsType
                                     && c.Value.Contains(EditPermission));

            try
            {
                var apiTask = this.Bind<ApiTask>();
                var id = logic.CreateTaskByTemplate(apiTask);
                var response = (Response) $"{id}";
                response.StatusCode = HttpStatusCode.OK;
                return response;
            }
            catch
            {
                return HttpStatusCode.InternalServerError;
            }
        }

        public SyncServerModule(ILogic logic, ServiceConfiguration config) : base(config)
        {
            //Get["/currentexporters"] = parameters =>
            //{
            //    try
            //    {
            //        var response = (Response) logic.GetAllTelegramChannelsJson();
            //        response.StatusCode = HttpStatusCode.OK;
            //        return response;
            //    }
            //    catch
            //    {
            //        return HttpStatusCode.InternalServerError;
            //    }
            //};

            Post(PostTaskTemplateRoute,
                parameters => CreateTaskByTemplate(logic),
                name: nameof(CreateTaskByTemplate));
        }
    }
}