using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ReportService.Interfaces.Core;
using System.Threading.Tasks;

namespace ReportService.Api.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class SiteController : ControllerBase
    {
        public const string GetAllTasksHtmlRoute = "tasks.html";
        public const string GetAllTaskInstancesHtmlRoute = "/site/tasks-{id}.html";
        public const string GetAllTasksInWorkHtmlRoute = "/site/tasks/inwork.html";
        public const string GetAllEntitiesCountHtmlRoute = "/site/entities.html";

        private readonly ILogic logic;
        public SiteController(ILogic logic)
        {
            this.logic = logic;
        }
               
        [HttpGet(GetAllTasksHtmlRoute)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ContentResult> GetAllTasksHtmlAsync()
        {
            try
            {
                var response = new ContentResult
                {
                    Content = $"{await logic.GetTasksList_HtmlPageAsync()}",
                    StatusCode = StatusCodes.Status200OK,
                    ContentType = "text/html"
                };

                return response;
            }
            catch
            {
                return new ContentResult
                {
                    Content = "Internal error during request execution",
                    StatusCode = StatusCodes.Status500InternalServerError
                };
            }
        }
    }
}