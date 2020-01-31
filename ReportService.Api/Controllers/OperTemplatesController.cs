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
    public class OperTemplatesController : BaseController
    {
        private readonly ILogic logic;
        public const string GetRegisteredImportersRoute = "registeredimporters";
        public const string GetRegisteredExportersRoute = "registeredexporters";
        public const string GetTaskOperationsRoute = "taskopers";
        public const string DeleteOperationTemplateRoute = "{templateid}";
        public const string PutOperationTemplateRoute = "{id}";

        public OperTemplatesController(ILogic logic)
        {
            this.logic = logic;
        }

        [HttpGet]
        public ContentResult GetAllOperationTemplates()
        {
            try
            {
                return GetSuccessfulResult(logic.GetAllOperTemplatesJson());
            }
            catch
            {
                return GetInternalErrorResult();
            }
        }

        [HttpGet(GetRegisteredImportersRoute)]
        public ContentResult GetImporters()
        {
            try
            {
                return GetSuccessfulResult(logic.GetAllRegisteredImportersJson());
            }
            catch
            {
                return GetInternalErrorResult();
            }
        }

        [HttpGet(GetRegisteredExportersRoute)]
        public ContentResult GetExporters()
        {
            try
            {
                return GetSuccessfulResult(logic.GetAllRegisteredExportersJson());
            }
            catch
            {
                return GetInternalErrorResult();
            }
        }

        [HttpGet(GetTaskOperationsRoute)]
        public ContentResult GetAllOperations()
        {
            try
            {
                return GetSuccessfulResult(logic.GetAllOperationsJson()); //todo: check if always needed all the data including configs,possibly do two methods
            }
            catch
            {
                return GetInternalErrorResult();
            }
        }

        [Authorize(Domain0Auth.Policy, Roles = "reporting.edit")]
        [HttpDelete(DeleteOperationTemplateRoute)]
        public IActionResult DeleteOperationTemplate(int templateid)
        {
            try
            {
                logic.DeleteOperationTemplate(templateid);
                return StatusCode(200);
            }
            catch
            {
                return StatusCode(500);
            }
        }

        [Authorize(Domain0Auth.Policy, Roles = "reporting.edit")]
        [HttpPost]
        public ContentResult CreateOperationTemplate([FromBody] DtoOperTemplate newOper)
        {
            try
            {
                var id = logic.CreateOperationTemplate(newOper);

                return GetSuccessfulResult(id.ToString());
            }
            catch
            {
                return GetInternalErrorResult();
            }
        }


        [Authorize(Domain0Auth.Policy, Roles = "reporting.edit")]
        [HttpPut(PutOperationTemplateRoute)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public ContentResult UpdateOperationTemplate(int id, [FromBody] DtoOperTemplate oper)
        {
            try
            {
                if (id != oper.Id)
                    return new ContentResult
                    {
                        Content = "Request id does not match template id",
                        StatusCode = StatusCodes.Status400BadRequest
                    };

                logic.UpdateOperationTemplate(oper);

                return new ContentResult { StatusCode = StatusCodes.Status200OK };
            }
            catch
            {
                return GetInternalErrorResult();
            }
        }

    }
}