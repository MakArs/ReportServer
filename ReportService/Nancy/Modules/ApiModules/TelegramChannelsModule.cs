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
    public sealed class TelegramChannelsModule : NancyBaseModule
    {
        public const string GetTelegramChannelsRoute = "/api/v2/telegrams";
        public const string PostTelegramChannelRoute = "/api/v2/telegrams";
        public const string PutTelegramChannelRoute = "/api/v2/telegrams/{id:int}";

        [Route(nameof(GetAllChannels))]
        [Route(HttpMethod.Get, GetTelegramChannelsRoute)]
        [Route(
            Tags = new[] {"Telegram Channels"},
            Summary = "Method for receiving all telegram channels in service")]
        [SwaggerResponse(
            HttpStatusCode.OK,
            Message = "Success",
            Model = typeof(List<DtoTelegramChannel>))]
        [SwaggerResponse(
            HttpStatusCode.InternalServerError,
            "Internal error during request execution")]
        public Response GetAllChannels(ILogic logic)
        {
            try
            {
                var response = (Response) logic.GetAllTelegramChannelsJson();
                response.StatusCode = HttpStatusCode.OK;
                return response;
            }
            catch
            {
                return HttpStatusCode.InternalServerError;
            }
        }

        [Route(nameof(CreateChannel))]
        [Route(HttpMethod.Post, PostTelegramChannelRoute)]
        [Route(Consumes = new[] {"application/json"})]
        [Route(
            Tags = new[] {"Telegram Channels"},
            Summary = "Method for creating telegram channel")]
        [RouteParam(
            ParamIn = ParameterIn.Body,
            Name = "telegram channel",
            ParamType = typeof(DtoTelegramChannel),
            Required = true,
            Description = "New telegram channel")]
        [SwaggerResponse(
            HttpStatusCode.OK,
            Message = "Success",
            Model = typeof(int))]
        [SwaggerResponse(
            HttpStatusCode.InternalServerError,
            "Internal error during request execution")]
        public Response CreateChannel(ILogic logic)
        {
            this.RequiresClaims(c => c.Type == PermissionsType
                                     && c.Value.Contains(EditPermission));
            try
            {
                var newTelegramChannel = this.Bind<DtoTelegramChannel>();
                var id = logic.CreateTelegramChannel(newTelegramChannel);
                var response = (Response) $"{id}";
                response.StatusCode = HttpStatusCode.OK;
                return response;
            }
            catch
            {
                return HttpStatusCode.InternalServerError;
            }
        }

        [Route(nameof(UpdateChannel))]
        [Route(HttpMethod.Put, PutTelegramChannelRoute)]
        [Route(Consumes = new[] {"application/json"})]
        [Route(
            Tags = new[] {"Telegram Channels"},
            Summary = "Method for updating telegram channel")]
        [RouteParam(
            ParamIn = ParameterIn.Body,
            Name = "telegram channel",
            ParamType = typeof(DtoTelegramChannel),
            Required = true,
            Description = "Existing telegram channel")]
        [RouteParam(
            ParamIn = ParameterIn.Path,
            Name = "id",
            ParamType = typeof(int),
            Required = true,
            Description = "Id of telegram channel that you need to update")]
        [SwaggerResponse(HttpStatusCode.OK, Message = "Success")]
        [SwaggerResponse(
            HttpStatusCode.BadRequest,
            "Post query id does not matches with query body id")]
        [SwaggerResponse(
            HttpStatusCode.InternalServerError,
            "Internal error during request execution")]
        public Response UpdateChannel(ILogic logic)
        {
            this.RequiresClaims(c => c.Type == PermissionsType
                                     && c.Value.Contains(EditPermission));
            try
            {
                var existingTelegramChannel = this.Bind<DtoTelegramChannel>
                    (new BindingConfig {BodyOnly = true});

                if (Context.Parameters.id != existingTelegramChannel.Id)
                    return HttpStatusCode.BadRequest;

                logic.UpdateTelegramChannel(existingTelegramChannel);
                return HttpStatusCode.OK;
            }
            catch
            {
                return HttpStatusCode.InternalServerError;
            }
        }

        public TelegramChannelsModule(ILogic logic)
        {
            this.RequiresAnyClaim(c => c.Type == PermissionsType
                                       && (c.Value.Contains(ViewPermission)
                                           || c.Value.Contains(StopRunPermission)
                                           || c.Value.Contains(EditPermission)));

            Get(GetTelegramChannelsRoute,
                _ => GetAllChannels(logic),
                name: nameof(GetAllChannels));

            Post(PostTelegramChannelRoute,
                parameters => CreateChannel(logic),
                name: nameof(CreateChannel));

            Put(PutTelegramChannelRoute,
                parameters => UpdateChannel(logic),
                name: nameof(UpdateChannel));
        }
    }
}