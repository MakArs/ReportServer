using System.Collections.Generic;
using Nancy;
using Nancy.Security;
using Nancy.Swagger.Annotations.Attributes;
using ReportService.Entities;
using ReportService.Interfaces.Core;
using ReportService.Nancy.Models;
using Swagger.ObjectModel;
using Response = Nancy.Response;

namespace ReportService.Nancy.Modules.ApiModules
{
    public sealed class InstancesModule : NancyBaseModule
    {
        public const string GetAllTaskInstancesRoute = "/api/v2/instances";
        public const string GetOperInstancesOfTaskInstanceRoute = "/api/v2/instances/{instanceid}/operinstances";
        public const string GetFullOperInstanceRoute = "/api/v2/instances/operinstances/{id}";
        public const string DeleteTaskInstanceRoute = "/api/v2/instances/{id}";


        [Route(nameof(GetAllTaskInstances))]
        [Route(HttpMethod.Get, GetAllTaskInstancesRoute)]
        [Route(Tags = new[] {"Instances"},
            Summary = "Method for receiving all task instances in service")]
        [RouteParam(
            ParamIn = ParameterIn.Header,
            Name = "Authorization",
            ParamType = typeof(string),
            Required = true,
            Description = "JWT access token")]
        [SwaggerResponse(
            HttpStatusCode.OK,
            Message = "Success",
            Model = typeof(List<DtoTaskInstance>))]
        [SwaggerResponse(
            HttpStatusCode.InternalServerError,
            "Internal error during request execution")]
        public Response GetAllTaskInstances(ILogic logic)
        {
            try
            {
                var response = (Response) logic.GetAllTaskInstancesJson();
                response.StatusCode = HttpStatusCode.OK;
                return response;
            }
            catch
            {
                return HttpStatusCode.InternalServerError;
            }
        }

        [Route(nameof(GetOperInstancesOfTaskInstance))]
        [Route(HttpMethod.Get, GetOperInstancesOfTaskInstanceRoute)]
        [Route(
            Tags = new[] {"Instances"},
            Summary =
                "Method for receiving all operation instances (without operation packages) of task instance by task instance ID")]
        [RouteParam(
            ParamIn = ParameterIn.Path,
            Name = "instanceid",
            ParamType = typeof(long),
            Required = true,
            Description = "Id of task instance which operation instances you are need")]
        [RouteParam(
            ParamIn = ParameterIn.Header,
            Name = "Authorization",
            ParamType = typeof(string),
            Required = true,
            Description = "JWT access token")]
        [SwaggerResponse(
            HttpStatusCode.OK,
            Message = "Success",
            Model = typeof(List<ApiOperInstance>))]
        [SwaggerResponse(
            HttpStatusCode.InternalServerError,
            "Internal error during request execution")]
        public Response GetOperInstancesOfTaskInstance(ILogic logic)
        {
            try
            {
                var response = (Response) logic
                    .GetOperInstancesByTaskInstanceIdJson(Context.Parameters.instanceid);
                response.StatusCode = HttpStatusCode.OK;
                return response;
            }
            catch
            {
                return HttpStatusCode.InternalServerError;
            }
        }

        [Route(nameof(GetFullOperInstance))]
        [Route(HttpMethod.Get, GetFullOperInstanceRoute)]
        [Route(Tags = new[] {"Instances"},
            Summary = "Method for receiving operation instance with operation packages by operation instance ID")]
        [RouteParam(
            ParamIn = ParameterIn.Path,
            Name = "id",
            ParamType = typeof(long),
            Required = true,
            Description = "Id of task operation that you are need")]
        [RouteParam(
            ParamIn = ParameterIn.Header,
            Name = "Authorization",
            ParamType = typeof(string),
            Required = true,
            Description = "JWT access token")]
        [SwaggerResponse(
            HttpStatusCode.OK,
            Message = "Success",
            Model = typeof(ApiOperInstance))]
        [SwaggerResponse(
            HttpStatusCode.InternalServerError,
            "Internal error during request execution")]
        public Response GetFullOperInstance(ILogic logic)
        {
            try
            {
                var response = (Response) logic.GetFullOperInstanceByIdJson(Context.Parameters.id);
                response.StatusCode = HttpStatusCode.OK;
                return response;
            }
            catch
            {
                return HttpStatusCode.InternalServerError;
            }
        }

        [Route(nameof(DeleteInstance))]
        [Route(HttpMethod.Delete, DeleteTaskInstanceRoute)]
        [Route(
            Tags = new[] {"Instances"},
            Summary = "Method for deleting task instance by ID")]
        [RouteParam(
            ParamIn = ParameterIn.Path,
            Name = "id",
            ParamType = typeof(long),
            Required = true,
            Description = "Id of task instance that you need to delete")]
        [RouteParam(
            ParamIn = ParameterIn.Header,
            Name = "Authorization",
            ParamType = typeof(string),
            Required = true,
            Description = "JWT access token")]
        [SwaggerResponse(HttpStatusCode.OK, Message = "Success")]
        [SwaggerResponse(
            HttpStatusCode.InternalServerError,
            "Internal error during request execution")]
        public Response DeleteInstance(ILogic logic)
        {
            this.RequiresClaims(c => c.Type == PermissionsType
                                     && c.Value.Contains(EditPermission));

            try
            {
                logic.DeleteTaskInstanceById(Context.Parameters.id);
                return HttpStatusCode.OK;
            }
            catch
            {
                return HttpStatusCode.InternalServerError;
            }
        }

        public InstancesModule(ILogic logic)
        {
            this.RequiresClaims(c => c.Type == PermissionsType
                                     && (c.Value.Contains(ViewPermission)
                                         || c.Value.Contains(StopRunPermission)
                                         || c.Value.Contains(EditPermission)));

            // TODO: filter - top, paginations
            Get(GetAllTaskInstancesRoute,
                parameters => GetAllTaskInstances(logic),
                name: nameof(GetAllTaskInstances));

            Get(GetOperInstancesOfTaskInstanceRoute,
                parameters => GetOperInstancesOfTaskInstance(logic),
                name: nameof(GetOperInstancesOfTaskInstance));

            Get(GetFullOperInstanceRoute,
                parameters => GetFullOperInstance(logic),
                name: nameof(GetFullOperInstance));

            Delete(DeleteTaskInstanceRoute,
                parameters => DeleteInstance(logic),
                name: nameof(DeleteInstance));
        }
    }
}