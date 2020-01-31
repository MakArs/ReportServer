using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ReportService.Interfaces.Core;
using System.Threading.Tasks;

namespace ReportService.Api.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class SiteController : BaseController
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
                return GetInternalErrorResult();
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
                return GetInternalErrorResult();
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
                return GetInternalErrorResult();
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
                return GetInternalErrorResult();
            }
        }
    }
}