using System.Threading.Tasks;
using Nancy;
using Nancy.Swagger.Annotations.Attributes;
using ReportService.Interfaces.Core;
using Swagger.ObjectModel;
using Response = Nancy.Response;

namespace ReportService.Nancy.Modules
{
    public sealed class SiteModule : NancyBaseModule
    {
        public const string GetAllTasksHtmlRoute = "/site/tasks.html";
        public const string GetAllTaskInstancesHtmlRoute = "/site/tasks-{id}.html";
        public const string GetAllTasksInWorkHtmlRoute = "/site/tasks/inwork.html";
        public const string GetAllEntitiesCountHtmlRoute = "/site/entities.html";

        [Route(nameof(GetAllTasksHtmlAsync))]
        [Route(HttpMethod.Get, GetAllTasksHtmlRoute)]
        [Route(
            Tags = new[] {"Site"},
            Summary = "Method for getting html page with table of all tasks in service")]
        [SwaggerResponse(
            HttpStatusCode.OK,
            Message = "Success",
            Model = typeof(string))]
        [SwaggerResponse(
            HttpStatusCode.InternalServerError,
            "Internal error during request execution")]
        public async Task<Response> GetAllTasksHtmlAsync(ILogic logic)
        {
            try
            {
                var response = (Response) $"{await logic.GetTasksList_HtmlPageAsync()}";
                response.ContentType = "text/html";
                response.StatusCode = HttpStatusCode.OK;
                return response;
            }
            catch
            {
                return HttpStatusCode.InternalServerError;
            }
        }

        [Route(nameof(GetAllTasksInWorkHtmlAsync))]
        [Route(HttpMethod.Get, GetAllTasksInWorkHtmlRoute)]
        [Route(
            Tags = new[] {"Site"},
            Summary = "Method for getting html page with table of all working tasks in service")]
        [SwaggerResponse(
            HttpStatusCode.OK,
            Message = "Success",
            Model = typeof(string))]
        [SwaggerResponse(
            HttpStatusCode.InternalServerError,
            "Internal error during request execution")]
        public async Task<Response> GetAllTasksInWorkHtmlAsync(ILogic logic)
        {
            try
            {
                string entities = await logic.GetTasksInWorkList_HtmlPageAsync();
                var response = (Response) entities;
                response.ContentType = "text/html";
                response.StatusCode = HttpStatusCode.OK;
                return response;
            }
            catch
            {
                return HttpStatusCode.InternalServerError;
            }
        }

        [Route(nameof(GetAllEntitiesCountHtml))]
        [Route(HttpMethod.Get, GetAllEntitiesCountHtmlRoute)]
        [Route(
            Tags = new[] {"Site"},
            Summary = "Method for getting html page with of all entities in service")]
        [SwaggerResponse(
            HttpStatusCode.OK,
            Message = "Success",
            Model = typeof(string))]
        [SwaggerResponse(
            HttpStatusCode.InternalServerError,
            "Internal error during request execution")]
        public Response GetAllEntitiesCountHtml(ILogic logic)
        {
            try
            {
                string entities = logic.GetEntitiesCountJson();
                var response = (Response) entities;
                response.StatusCode = HttpStatusCode.OK;
                return response;
            }
            catch
            {
                return HttpStatusCode.InternalServerError;
            }
        }

        [Route(nameof(GetAllTaskInstancesHtmlAsync))]
        [Route(HttpMethod.Get, GetAllTaskInstancesHtmlRoute)]
        [Route(
            Tags = new[] {"Site"},
            Summary = "Method for getting html page with table of all task instances in service by task id")]
        [RouteParam(
            ParamIn = ParameterIn.Path,
            Name = "id",
            ParamType = typeof(long),
            Required = true,
            Description = "Id of task which instances you are need")]
        [SwaggerResponse(
            HttpStatusCode.OK,
            Message = "Success",
            Model = typeof(string))]
        [SwaggerResponse(
            HttpStatusCode.InternalServerError,
            "Internal error during request execution")]
        public async Task<Response> GetAllTaskInstancesHtmlAsync(ILogic logic)
        {
            try
            {
                var response = (Response) $@"{await logic
                    .GetFullInstanceList_HtmlPageAsync(Context.Parameters.id)}";
                response.ContentType = "text/html";
                response.StatusCode = HttpStatusCode.OK;
                return response;
            }
            catch
            {
                return HttpStatusCode.InternalServerError;
            }
        }

        public SiteModule(ILogic logic)
        {
            Get(GetAllTasksHtmlRoute,
                async (parameters, token) => await GetAllTasksHtmlAsync(logic),
                name: nameof(GetAllTasksHtmlAsync));

            Get(GetAllTasksInWorkHtmlRoute,
                async (parameters, token) => await GetAllTasksInWorkHtmlAsync(logic),
                name: nameof(GetAllTasksInWorkHtmlAsync));

            Get(GetAllEntitiesCountHtmlRoute,
                parameters => GetAllEntitiesCountHtml(logic),
                name: nameof(GetAllEntitiesCountHtml));

            Get(GetAllTaskInstancesHtmlRoute,
                async (parameters, token) => await GetAllTaskInstancesHtmlAsync(logic),
                name: nameof(GetAllTaskInstancesHtmlAsync));
        }
    } //class
}