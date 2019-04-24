using System.Collections.Generic;
using Nancy;
using Nancy.ModelBinding;
using Nancy.Security;
using Nancy.Swagger.Annotations.Attributes;
using ReportService.Interfaces.Core;
using Swagger.ObjectModel;
using Response = Nancy.Response;

namespace ReportService.Nancy.Modules.ApiModules
{
    public sealed class OperationTemplatesModule : NancyBaseModule
    {
        public const string GetOperationTemplatesRoute = "/api/v2/opertemplates";
        public const string GetRegisteredImportersRoute = "/api/v2/opertemplates/registeredimporters";
        public const string GetRegisteredExportersRoute = "/api/v2/opertemplates/registeredexporters";
        public const string GetTaskOperationsRoute = "/api/v2/opertemplates/taskopers";
        public const string DeleteOperationTemplateRoute = "/api/v2/opertemplates/{id}";
        public const string PostOperationTemplateRoute = "/api/v2/opertemplates";
        public const string PutOperationTemplateRoute = "/api/v2/opertemplates/{id}";

        [Route(nameof(GetAllTemplates))]
        [Route(HttpMethod.Get, GetOperationTemplatesRoute)]
        [Route(
            Tags = new[] {"Operation templates"},
            Summary = "Method for receiving all operation templates in service")]
        [RouteParam(
            ParamIn = ParameterIn.Header,
            Name = "Authorization",
            ParamType = typeof(string),
            Required = true,
            Description = "JWT access token")]
        [SwaggerResponse(
            HttpStatusCode.OK,
            Message = "Success",
            Model = typeof(List<DtoOperTemplate>))]
        [SwaggerResponse(
            HttpStatusCode.InternalServerError,
            "Internal error during request execution")]
        public Response GetAllTemplates(ILogic logic)
        {
            try
            {
                var response = (Response) logic.GetAllOperTemplatesJson();
                response.StatusCode = HttpStatusCode.OK;
                return response;
            }
            catch
            {
                return HttpStatusCode.InternalServerError;
            }
        }

        [Route(nameof(GetImporters))]
        [Route(HttpMethod.Get, GetRegisteredImportersRoute)]
        [Route(
            Tags = new[] {"Operation templates"},
            Summary = "Method for receiving all importer types registered in service")]
        [RouteParam(
            ParamIn = ParameterIn.Header,
            Name = "Authorization",
            ParamType = typeof(string),
            Required = true,
            Description = "JWT access token")]
        [SwaggerResponse(
            HttpStatusCode.OK,
            Message = "Success",
            Model = typeof(Dictionary<string, string>))]
        [SwaggerResponse(
            HttpStatusCode.InternalServerError,
            "Internal error during request execution")]
        public Response GetImporters(ILogic logic)
        {
            try
            {
                var response = (Response) logic.GetAllRegisteredImportersJson();
                response.StatusCode = HttpStatusCode.OK;
                return response;
            }
            catch
            {
                return HttpStatusCode.InternalServerError;
            }
        }

        [Route(nameof(GetExporters))]
        [Route(HttpMethod.Get, GetRegisteredExportersRoute)]
        [Route(
            Tags = new[] {"Operation templates"},
            Summary = "Method for receiving all exporter types registered in service")]
        [RouteParam(
            ParamIn = ParameterIn.Header,
            Name = "Authorization",
            ParamType = typeof(string),
            Required = true,
            Description = "JWT access token")]
        [SwaggerResponse(
            HttpStatusCode.OK,
            Message = "Success",
            Model = typeof(Dictionary<string, string>))]
        [SwaggerResponse(
            HttpStatusCode.InternalServerError,
            "Internal error during request execution")]
        public Response GetExporters(ILogic logic)
        {
            try
            {
                var response = (Response) logic.GetAllRegisteredExportersJson();
                response.StatusCode = HttpStatusCode.OK;
                return response;
            }
            catch
            {
                return HttpStatusCode.InternalServerError;
            }
        }

        [Route(nameof(GetAllOperations))]
        [Route(HttpMethod.Get, GetTaskOperationsRoute)]
        [Route(
            Tags = new[] {"Operation templates"},
            Summary = "Method for receiving all operations binded to tasks in service")]
        [RouteParam(
            ParamIn = ParameterIn.Header,
            Name = "Authorization",
            ParamType = typeof(string),
            Required = true,
            Description = "JWT access token")]
        [SwaggerResponse(
            HttpStatusCode.OK,
            Message = "Success",
            Model = typeof(List<DtoOperation>))]
        [SwaggerResponse(
            HttpStatusCode.InternalServerError,
            "Internal error during request execution")]
        public Response GetAllOperations(ILogic logic)
        {
            try
            {
                var response = (Response) logic.GetAllOperationsJson();
                response.StatusCode = HttpStatusCode.OK;
                return response;
            }
            catch
            {
                return HttpStatusCode.InternalServerError;
            }
        }

        [Route(nameof(DeleteTemplate))]
        [Route(HttpMethod.Delete, DeleteOperationTemplateRoute)]
        [Route(
            Tags = new[] {"Operation templates"},
            Summary = "Method for deleting operation template by ID")]
        [RouteParam(
            ParamIn = ParameterIn.Path,
            Name = "id",
            ParamType = typeof(int),
            Required = true,
            Description = "Id of operation template that you need to delete")]
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
        public Response DeleteTemplate(ILogic logic)
        {
            this.RequiresClaims(c => c.Type == PermissionsType
                                     && c.Value.Contains(EditPermission));
            try
            {
                logic.DeleteOperationTemplate(Context.Parameters.id);
                return HttpStatusCode.OK;
            }
            catch
            {
                return HttpStatusCode.InternalServerError;
            }
        }

        [Route(nameof(CreateTemplate))]
        [Route(HttpMethod.Post, PostOperationTemplateRoute)]
        [Route(Consumes = new[] {"application/json"})]
        [Route(
            Tags = new[] {"Operation templates"},
            Summary = "Method for creating operation template")]
        [RouteParam(
            ParamIn = ParameterIn.Body,
            Name = "operation template",
            ParamType = typeof(DtoOperTemplate),
            Required = true,
            Description = "New operation template")]
        [RouteParam(
            ParamIn = ParameterIn.Header,
            Name = "Authorization",
            ParamType = typeof(string),
            Required = true,
            Description = "JWT access token")]
        [SwaggerResponse(
            HttpStatusCode.OK,
            Message = "Success",
            Model = typeof(int))]
        [SwaggerResponse(
            HttpStatusCode.InternalServerError,
            "Internal error during request execution")]
        public Response CreateTemplate(ILogic logic)
        {
            this.RequiresClaims(c => c.Type == PermissionsType
                                     && c.Value.Contains(EditPermission));
            try
            {
                var newOper = this.Bind<DtoOperTemplate>();
                var id = logic.CreateOperationTemplate(newOper);
                var response = (Response) $"{id}";
                response.StatusCode = HttpStatusCode.OK;
                return response;
            }
            catch
            {
                return HttpStatusCode.InternalServerError;
            }
        }

        [Route(nameof(UpdateTemplate))]
        [Route(HttpMethod.Put, PutOperationTemplateRoute)]
        [Route(Consumes = new[] {"application/json"})]
        [Route(
            Tags = new[] {"Operation templates"},
            Summary = "Method for updating operation template")]
        [RouteParam(
            ParamIn = ParameterIn.Body,
            Name = "operation template",
            ParamType = typeof(DtoOperTemplate),
            Required = true,
            Description = "Existing operation template")]
        [RouteParam(
            ParamIn = ParameterIn.Path,
            Name = "id",
            ParamType = typeof(int),
            Required = true,
            Description = "Id of operation template that you need to update")]
        [RouteParam(
            ParamIn = ParameterIn.Header,
            Name = "Authorization",
            ParamType = typeof(string),
            Required = true,
            Description = "JWT access token")]
        [SwaggerResponse(HttpStatusCode.OK, Message = "Success")]
        [SwaggerResponse(
            HttpStatusCode.BadRequest,
            "Post query id does not matches with query body id")]
        [SwaggerResponse(
            HttpStatusCode.InternalServerError,
            "Internal error during request execution")]
        public Response UpdateTemplate(ILogic logic)
        {
            this.RequiresClaims(c => c.Type == PermissionsType
                                     && c.Value.Contains(EditPermission));
            try
            {
                var existingOper = this.Bind<DtoOperTemplate>
                    (new BindingConfig {BodyOnly = true});

                if (int.Parse(Context.Parameters.id) != existingOper.Id)
                    return HttpStatusCode.BadRequest;

                logic.UpdateOperationTemplate(existingOper);
                return HttpStatusCode.OK;
            }
            catch
            {
                return HttpStatusCode.InternalServerError;
            }
        }

        public OperationTemplatesModule(ILogic logic)
        {
            //can do through RequiresAnyClaim but more code
            this.RequiresClaims(c => c.Type == PermissionsType
                                     && (c.Value.Contains(ViewPermission)
                                         || c.Value.Contains(StopRunPermission)
                                         || c.Value.Contains(EditPermission)));

            Get(GetOperationTemplatesRoute,
                _ => GetAllTemplates(logic),
                name: nameof(GetAllTemplates));

            Get(GetRegisteredImportersRoute,
                _ => GetImporters(logic),
                name: nameof(GetImporters));

            Get(GetRegisteredExportersRoute,
                _ => GetExporters(logic),
                name: nameof(GetExporters));

            Get(GetTaskOperationsRoute,
                _ => GetAllOperations(logic),
                name: nameof(GetAllOperations));

            Delete(DeleteOperationTemplateRoute,
                parameters => DeleteTemplate(logic),
                name: nameof(DeleteTemplate));

            Post(PostOperationTemplateRoute,
                parameters => CreateTemplate(logic),
                name: nameof(CreateTemplate));

            Put(PutOperationTemplateRoute,
                parameters => UpdateTemplate(logic),
                name: nameof(UpdateTemplate));
        }
    }
}