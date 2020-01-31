using Domain0.Tokens;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ReportService.Entities.Dto;
using ReportService.Interfaces.Core;

namespace ReportService.Api.Controllers
{
    [Route("api/v2/[controller]")]
    [ApiController]
    public class RecipientGroupsController : BaseController
    {
        private readonly ILogic logic;
        public const string DeleteRecepientGroupRoute = "{id}";
        public const string PutRecepientGroupRoute = "{id}";

        public RecipientGroupsController(ILogic logic)
        {
            this.logic = logic;
        }

        [HttpGet]
        public ContentResult GetAllRecipientGroups()
        {
            try
            {
                return GetSuccessfulResult(logic.GetAllRecepientGroupsJson());
            }
            catch
            {
                return GetInternalErrorResult();
            }
        }

        [Authorize(Domain0Auth.Policy, Roles = "reporting.edit")]
        [HttpDelete(DeleteRecepientGroupRoute)]
        public IActionResult DeleteRecipientGroup(int id)
        {
            try
            {
                logic.DeleteRecepientGroup(id);
                return StatusCode(200);
            }
            catch
            {
                return StatusCode(500);
            }
        }

        [Authorize(Domain0Auth.Policy, Roles = "reporting.edit")]
        [HttpPost]
        public ContentResult CreateRecipientGroup([FromBody] DtoRecepientGroup newGroup)
        {
            try
            {
                var id = logic.CreateRecepientGroup(newGroup);

                return GetSuccessfulResult(id.ToString());
            }
            catch
            {
                return GetInternalErrorResult();
            }
        }

        [Authorize(Domain0Auth.Policy, Roles = "reporting.edit")]
        [HttpPut(PutRecepientGroupRoute)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public ContentResult UpdateRecipientGroup(int id, [FromBody] DtoRecepientGroup group)
        {
            try
            {
                if (id != group.Id)
                    return new ContentResult
                    {
                        Content = "Request id does not match group id",
                        StatusCode = StatusCodes.Status400BadRequest
                    };

                logic.UpdateRecepientGroup(group);

                return new ContentResult { StatusCode = StatusCodes.Status200OK };
            }
            catch
            {
                return GetInternalErrorResult();
            }
        }
    }
}