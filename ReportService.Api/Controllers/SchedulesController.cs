using Domain0.Tokens;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ReportService.Entities.Dto;
using ReportService.Interfaces.Core;

namespace ReportService.Api.Controllers
{
    [Authorize(Domain0Auth.Policy, Roles = "reporting.view, reporting.stoprun, reporting.edit")]
    [Route("api/v2/[controller]")]
    [ApiController]
    public class SchedulesController : BaseController
    {
        private readonly ILogic logic;
        public const string DeleteScheduleRoute = "{id}";
        public const string PutScheduleRoute = "{id}";

        public SchedulesController(ILogic logic)
        {
            this.logic = logic;
        }

        [HttpGet]
        public ContentResult GetAllSchedules()
        {
            try
            {
                return GetSuccessfulResult(logic.GetAllSchedulesJson());
            }
            catch
            {
                return GetInternalErrorResult();
            }
        }

        [Authorize(Domain0Auth.Policy, Roles = "reporting.edit")]
        [HttpDelete(DeleteScheduleRoute)]
        public IActionResult DeleteSchedule(int id)
        {
            try
            {
                logic.DeleteSchedule(id);
                return StatusCode(200);
            }
            catch
            {
                return StatusCode(500);
            }
        }

        [Authorize(Domain0Auth.Policy, Roles = "reporting.edit")]
        [HttpPost]
        public ContentResult CreateRecipientGroup([FromBody] DtoSchedule newSchedule)
        {
            try
            {
                var id = logic.CreateSchedule(newSchedule);

                return GetSuccessfulResult(id.ToString());
            }
            catch
            {
                return GetInternalErrorResult();
            }
        }

        [Authorize(Domain0Auth.Policy, Roles = "reporting.edit")]
        [HttpPut(PutScheduleRoute)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public ContentResult UpdateRecipientGroup(int id, [FromBody] DtoSchedule schedule)
        {
            try
            {
                if (id != schedule.Id)
                    return new ContentResult
                    {
                        Content = "Request id does not match schedule id",
                        StatusCode = StatusCodes.Status400BadRequest
                    };

                logic.UpdateSchedule(schedule);

                return new ContentResult {StatusCode = StatusCodes.Status200OK};
            }
            catch
            {
                return GetInternalErrorResult();
            }
        }
    }
}