using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ReportService.Interfaces.Core;
using System.Threading.Tasks;

namespace ReportService.Api.Controllers
{
    [Route("[controller]")]
    [ApiController]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public class SiteController : ControllerBase
    {
        private const string GetAllTasksHtmlRoute = "tasks.html";
        private const string GetAllTaskInstancesHtmlRoute = "tasks-{id}.html";
        private const string GetAllTasksInWorkHtmlRoute = "tasks/inwork.html";
        private const string GetAllEntitiesCountJsonRoute = "entities.json";

        private readonly ILogic logic;
        public SiteController(ILogic logic)
        {
            this.logic = logic;
        }

        [HttpGet(GetAllTasksHtmlRoute)]
        public async Task<ContentResult> GetAllTasksHtmlAsync()
        {
            try
            {
                var response = new ContentResult
                {
                    Content = await logic.GetTasksList_HtmlPageAsync(),
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

        [HttpGet(GetAllTaskInstancesHtmlRoute)]
        public async Task<ContentResult> GetAllTaskInstancesHtmlAsync(long id)
        {
            try
            {
                var response = new ContentResult
                {
                    Content = await logic.GetFullInstanceList_HtmlPageAsync(id),
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

        [HttpGet(GetAllTasksInWorkHtmlRoute)]
        public async Task<ContentResult> GetAllTasksInWorkHtmlAsync()
        {
            try
            {
                var response = new ContentResult
                {
                    Content = await logic.GetTasksInWorkList_HtmlPageAsync(),
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

        [HttpGet(GetAllEntitiesCountJsonRoute)]
        public ContentResult GetAllEntitiesCountJson()
        {
            try
            {
                var response = new ContentResult
                {
                    Content = logic.GetEntitiesCountJson(),
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