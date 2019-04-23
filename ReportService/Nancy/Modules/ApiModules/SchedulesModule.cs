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
    public sealed class SchedulesModule : NancyBaseModule
    {
        public const string GetSchedulesRoute = "/api/v2/schedules";
        public const string DeleteScheduleRoute = "/api/v2/schedules/{id:int}";
        public const string PostScheduleRoute = "/api/v2/schedules";
        public const string PutScheduleRoute = "/api/v2/schedules/{id:int}";

        [Route(nameof(GetAllSchedules))]
        [Route(HttpMethod.Get, GetSchedulesRoute)]
        [Route(
            Tags = new[] {"Schedules"},
            Summary = "Method for receiving all schedules in service")]
        [SwaggerResponse(
            HttpStatusCode.OK,
            Message = "Success",
            Model = typeof(List<DtoSchedule>))]
        [SwaggerResponse(
            HttpStatusCode.InternalServerError,
            "Internal error during request execution")]
        public Response GetAllSchedules(ILogic logic)
        {
            try
            {
                var response = (Response) logic.GetAllSchedulesJson();
                response.StatusCode = HttpStatusCode.OK;
                return response;
            }
            catch
            {
                return HttpStatusCode.InternalServerError;
            }
        }

        [Route(nameof(DeleteSchedule))]
        [Route(HttpMethod.Delete, DeleteScheduleRoute)]
        [Route(
            Tags = new[] {"Schedules"},
            Summary = "Method for deleting schedule by ID")]
        [RouteParam(
            ParamIn = ParameterIn.Path,
            Name = "id",
            ParamType = typeof(int),
            Required = true,
            Description = "Id of schedule that you need to delete")]
        [SwaggerResponse(HttpStatusCode.OK, Message = "Success")]
        [SwaggerResponse(
            HttpStatusCode.InternalServerError,
            "Internal error during request execution")]
        public Response DeleteSchedule(ILogic logic)
        {
            this.RequiresClaims(c => c.Type == PermissionsType
                                     && c.Value.Contains(EditPermission));

            try
            {
                logic.DeleteSchedule(Context.Parameters.id);
                return HttpStatusCode.OK;
            }
            catch
            {
                return HttpStatusCode.InternalServerError;
            }
        }

        [Route(nameof(CreateSchedule))]
        [Route(HttpMethod.Post, PostScheduleRoute)]
        [Route(Consumes = new[] {"application/json"})]
        [Route(
            Tags = new[] {"Schedules"},
            Summary = "Method for creating schedule")]
        [RouteParam(
            ParamIn = ParameterIn.Body,
            Name = "schedule",
            ParamType = typeof(DtoSchedule),
            Required = true,
            Description = "New schedule")]
        [SwaggerResponse(
            HttpStatusCode.OK,
            Message = "Success",
            Model = typeof(int))]
        [SwaggerResponse(
            HttpStatusCode.InternalServerError,
            "Internal error during request execution")]
        public Response CreateSchedule(ILogic logic)
        {
            this.RequiresClaims(c => c.Type == PermissionsType
                                     && c.Value.Contains(EditPermission));

            try
            {
                var newSchedule = this.Bind<DtoSchedule>();
                var id = logic.CreateSchedule(newSchedule);
                var response = (Response) $"{id}";
                response.StatusCode = HttpStatusCode.OK;
                return response;
            }
            catch
            {
                return HttpStatusCode.InternalServerError;
            }
        }

        [Route(nameof(UpdateSchedule))]
        [Route(HttpMethod.Put, PutScheduleRoute)]
        [Route(Consumes = new[] {"application/json"})]
        [Route(
            Tags = new[] {"Schedules"},
            Summary = "Method for updating schedule")]
        [RouteParam(
            ParamIn = ParameterIn.Body,
            Name = "schedule",
            ParamType = typeof(DtoSchedule),
            Required = true,
            Description = "Existing schedule")]
        [RouteParam(
            ParamIn = ParameterIn.Path,
            Name = "id",
            ParamType = typeof(int),
            Required = true,
            Description = "Id of schedule that you need to update")]
        [SwaggerResponse(HttpStatusCode.OK, Message = "Success")]
        [SwaggerResponse(
            HttpStatusCode.BadRequest,
            "Post query id does not matches with query body id")]
        [SwaggerResponse(
            HttpStatusCode.InternalServerError,
            "Internal error during request execution")]
        public Response UpdateSchedule(ILogic logic)
        {
            this.RequiresClaims(c => c.Type == PermissionsType
                                     && c.Value.Contains(EditPermission));

            try
            {
                var existingSchedule = this.Bind<DtoSchedule>
                    (new BindingConfig {BodyOnly = true});

                if (Context.Parameters.id != existingSchedule.Id)
                    return HttpStatusCode.BadRequest;

                logic.UpdateSchedule(existingSchedule);
                return HttpStatusCode.OK;
            }
            catch
            {
                return HttpStatusCode.InternalServerError;
            }
        }

        public SchedulesModule(ILogic logic)
        {
            this.RequiresClaims(c => c.Type == PermissionsType
                                     && (c.Value.Contains(ViewPermission)
                                         || c.Value.Contains(StopRunPermission)
                                         || c.Value.Contains(EditPermission)));

            Get(GetSchedulesRoute,
                _ => GetAllSchedules(logic),
                name: nameof(GetAllSchedules));

            Delete(DeleteScheduleRoute,
                parameters => DeleteSchedule(logic),
                name: nameof(DeleteSchedule));

            Post(PostScheduleRoute,
                parameters => CreateSchedule(logic),
                name: nameof(CreateSchedule));

            Put(PutScheduleRoute,
                parameters => UpdateSchedule(logic),
                name: nameof(UpdateSchedule));
        }
    }
}