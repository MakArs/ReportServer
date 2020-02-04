using Domain0.Tokens;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ReportService.Entities.Dto;
using ReportService.Interfaces.Core;

namespace ReportService.Api.Controllers
{
    [Authorize(Domain0Auth.Policy, Roles = "reporting.view, reporting.stoprun, reporting.edit")]
    [Route("api/v3/[controller]")]
    [ApiController]
    public class TelegramChannelsController : BaseController
    {
        private readonly ILogic logic;
        private const string PutTelegramChannelRoute = "{id}";

        public TelegramChannelsController(ILogic logic)
        {
            this.logic = logic;
        }

        [HttpGet]
        public ContentResult GetAllTelegramChannels()
        {
            try
            {
                return GetSuccessfulResult(logic.GetAllTelegramChannelsJson());
            }
            catch
            {
                return GetInternalErrorResult();
            }
        }

        [Authorize(Domain0Auth.Policy, Roles = "reporting.edit")]
        [HttpPost]
        public ContentResult CreateTelegramChannel([FromBody] DtoTelegramChannel newChannel)
        {
            try
            {
                var id = logic.CreateTelegramChannel(newChannel);

                return GetSuccessfulResult(id.ToString());
            }
            catch
            {
                return GetInternalErrorResult();
            }
        }

        [Authorize(Domain0Auth.Policy, Roles = "reporting.edit")]
        [HttpPut(PutTelegramChannelRoute)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public ContentResult UpdateTelegramChannel(int id, [FromBody] DtoTelegramChannel channel)
        {
            try
            {
                if (id != channel.Id)
                    return new ContentResult
                    {
                        Content = "Request id does not match channel id",
                        StatusCode = StatusCodes.Status400BadRequest
                    };

                logic.UpdateTelegramChannel(channel);

                return new ContentResult {StatusCode = StatusCodes.Status200OK};
            }
            catch
            {
                return GetInternalErrorResult();
            }
        }
    }
}