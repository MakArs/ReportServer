using System.Collections.Generic;
using Nancy;
using Nancy.ModelBinding;
using Nancy.Security;
using Nancy.Swagger.Annotations.Attributes;
using ReportService.Entities;
using ReportService.Entities.Dto;
using ReportService.Interfaces.Core;
using Swagger.ObjectModel;
using Response = Nancy.Response;

namespace ReportService.Nancy.Modules.ApiModules
{
    public sealed class RecipientGroupsModule : NancyBaseModule
    {
        public const string GetRecepientGroupsRoute = "/api/v2/recepientgroups";
        public const string DeleteRecepientGroupRoute = "/api/v2/recepientgroups/{id}";
        public const string PostRecepientGroupRoute = "/api/v2/recepientgroups";
        public const string PutRecepientGroupRoute = "/api/v2/recepientgroups/{id}";

        [Route(nameof(GetAllGroups))]
        [Route(HttpMethod.Get, GetRecepientGroupsRoute)]
        [Route(
            Tags = new[] {"Recipient groups"},
            Summary = "Method for receiving all recipient groups in service")]
        [RouteParam(
            ParamIn = ParameterIn.Header,
            Name = "Authorization",
            ParamType = typeof(string),
            Required = true,
            Description = "JWT access token")]
        [SwaggerResponse(
            HttpStatusCode.OK,
            Message = "Success",
            Model = typeof(List<DtoRecepientGroup>))]
        [SwaggerResponse(
            HttpStatusCode.InternalServerError,
            "Internal error during request execution")]
        public Response GetAllGroups(ILogic logic)
        {
            try
            {
                var response = (Response) logic.GetAllRecepientGroupsJson();
                response.StatusCode = HttpStatusCode.OK;
                return response;
            }
            catch
            {
                return HttpStatusCode.InternalServerError;
            }
        }

        [Route(nameof(DeleteGroup))]
        [Route(HttpMethod.Delete, DeleteRecepientGroupRoute)]
        [Route(
            Tags = new[] {"Recipient groups"},
            Summary = "Method for deleting recipient group by ID")]
        [RouteParam(
            ParamIn = ParameterIn.Path,
            Name = "id",
            ParamType = typeof(int),
            Required = true,
            Description = "Id of recipient group that you need to delete")]
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
        public Response DeleteGroup(ILogic logic)
        {
            this.RequiresClaims(c => c.Type == PermissionsType
                                     && c.Value.Contains(EditPermission));
            try
            {
                logic.DeleteRecepientGroup(Context.Parameters.id);
                return HttpStatusCode.OK;
            }
            catch
            {
                return HttpStatusCode.InternalServerError;
            }
        }

        [Route(nameof(CreateGroup))]
        [Route(HttpMethod.Post, PostRecepientGroupRoute)]
        [Route(Consumes = new[] {"application/json"})]
        [Route(
            Tags = new[] {"Recipient groups"},
            Summary = "Method for creating recipient group")]
        [RouteParam(
            ParamIn = ParameterIn.Body,
            Name = "recipient group",
            ParamType = typeof(DtoRecepientGroup),
            Required = true,
            Description = "New recipient group")]
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
        public Response CreateGroup(ILogic logic)
        {
            this.RequiresClaims(c => c.Type == PermissionsType
                                     && c.Value.Contains(EditPermission));
            try
            {
                var newReport = this.Bind<DtoRecepientGroup>();
                var id = logic.CreateRecepientGroup(newReport);
                var response = (Response) $"{id}";
                response.StatusCode = HttpStatusCode.OK;
                return response;
            }
            catch
            {
                return HttpStatusCode.InternalServerError;
            }
        }

        [Route(nameof(UpdateGroup))]
        [Route(HttpMethod.Put, PutRecepientGroupRoute)]
        [Route(Consumes = new[] {"application/json"})]
        [Route(
            Tags = new[] {"Recipient groups"},
            Summary = "Method for updating recipient group")]
        [RouteParam(
            ParamIn = ParameterIn.Body,
            Name = "recipient group",
            ParamType = typeof(DtoRecepientGroup),
            Required = true,
            Description = "Existing recipient group")]
        [RouteParam(
            ParamIn = ParameterIn.Path,
            Name = "id",
            ParamType = typeof(int),
            Required = true,
            Description = "Id of recipient group that you need to update")]
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
        public Response UpdateGroup(ILogic logic)
        {
            this.RequiresClaims(c => c.Type == PermissionsType
                                     && c.Value.Contains(EditPermission));

            try
            {
                var existingGroup = this.Bind<DtoRecepientGroup>
                    (new BindingConfig {BodyOnly = true});

                if (int.Parse(Context.Parameters.id) != existingGroup.Id)
                    return HttpStatusCode.BadRequest;

                logic.UpdateRecepientGroup(existingGroup);
                return HttpStatusCode.OK;
            }
            catch
            {
                return HttpStatusCode.InternalServerError;
            }
        }

        public RecipientGroupsModule(ILogic logic)
        {
            this.RequiresAnyClaim(c => c.Type == PermissionsType
                                       && (c.Value.Contains(ViewPermission)
                                           || c.Value.Contains(StopRunPermission)
                                           || c.Value.Contains(EditPermission)));

            Get(GetRecepientGroupsRoute,
                _ => GetAllGroups(logic),
                name: nameof(GetAllGroups));

            Delete(DeleteRecepientGroupRoute,
                parameters => DeleteGroup(logic),
                name: nameof(DeleteGroup));

            Post(PostRecepientGroupRoute,
                parameters => CreateGroup(logic),
                name: nameof(CreateGroup));

            Put(PutRecepientGroupRoute,
                parameters => UpdateGroup(logic),
                name: nameof(UpdateGroup));
        }
    }
}