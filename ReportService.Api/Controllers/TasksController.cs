using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Domain0.Tokens;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using ReportService.Api.Models;
using ReportService.Entities.Dto;
using ReportService.Interfaces.Core;

namespace ReportService.Api.Controllers
{
    [Authorize(Domain0Auth.Policy, Roles = "reporting.view, reporting.stoprun, reporting.edit")]
    [Route("api/v3/[controller]")]
    [ApiController]
    public class TasksController : BaseController
    {
        private readonly ILogic logic;
        private readonly IMapper mapper;
        private const string StopTaskInstanceRoute = "stop/{taskinstanceid}";
        private const string GetTaskInstancesRoute = "{id}/instances";
        private const string GetCurrentTaskViewRoute = "{id}/currentviews";
        private const string RunTasksRoute = "run/{id}";
        private const string GetWorkingTaskInstancesRoute = "working-{id}";
        private const string DeleteTaskRoute = "{id}";
        private const string PutTaskRoute = "{id}";

        public TasksController(ILogic logic, IMapper mapper)
        {
            this.logic = logic;
            this.mapper = mapper;
        }

        [HttpGet]
        public ContentResult GetAllTasks()
        {
            try
            {
                var reportTasks = logic.GetAllTasksJson();
                var apiTasks = reportTasks.Select(task => mapper.Map<ApiTask>(task));

                return GetSuccessfulResult(JsonConvert.SerializeObject(apiTasks));
            }
            catch
            {
                return GetInternalErrorResult();
            }
        }

        [Authorize(Domain0Auth.Policy, Roles = "reporting.stoprun, reporting.edit")]
        [HttpGet(StopTaskInstanceRoute)]
        public async Task<ContentResult> StopInstanceAsync(long taskinstanceid)
        {
            try
            {
                var stopped = await logic.StopTaskInstanceAsync(taskinstanceid);

                return GetSuccessfulResult(stopped.ToString());
            }
            catch
            {
                return GetInternalErrorResult();
            }
        }

        [HttpGet(GetTaskInstancesRoute)]
        public async Task<ContentResult> GetTaskInstances(long id)
        {
            try
            {
                return GetSuccessfulResult(await logic.GetAllTaskInstancesJson(id));
            }
            catch
            {
                return GetInternalErrorResult();
            }
        }

        [HttpGet(GetCurrentTaskViewRoute)]
        public async Task<ContentResult> GetTaskViewAsync(long id)
        {
            try
            {
                var view = await logic.GetCurrentViewAsync(id);

                return GetSuccessfulResult(view);
            }
            catch
            {
                return GetInternalErrorResult();
            }
        }


        [Authorize(Domain0Auth.Policy, Roles = "reporting.stoprun, reporting.edit")]
        [HttpGet(RunTasksRoute)]
        public ContentResult RunTask(long id)
        {
            try
            {
                var sentReps = logic.ForceExecute(id);

                return GetSuccessfulResult(sentReps);
            }
            catch
            {
                return GetInternalErrorResult();
            }
        }

        [HttpGet(GetWorkingTaskInstancesRoute)]
        public ContentResult GetWorkingTaskInstances(long id)
        {
            try
            {
                var sentReps = logic.GetWorkingTaskInstancesJson(id);

                return GetSuccessfulResult(sentReps);
            }
            catch
            {
                return GetInternalErrorResult();
            }
        }

        [Authorize(Domain0Auth.Policy, Roles = "reporting.edit")]
        [HttpDelete(DeleteTaskRoute)]
        public IActionResult DeleteTask(long id)
        {
            try
            {
                logic.DeleteTask(id);
                return StatusCode(200);
            }
            catch
            {
                return StatusCode(500);
            }
        }

        [Authorize(Domain0Auth.Policy, Roles = "reporting.edit")]
        [HttpPost]
        public ContentResult CreateTask([FromBody] ApiTask newTask)
        {
            try
            {
                var id = logic.CreateTask(mapper.Map<DtoTask>(newTask), newTask.BindedOpers);

                return GetSuccessfulResult(id.ToString());
            }
            catch
            {
                return GetInternalErrorResult();
            }
        }

        [Authorize(Domain0Auth.Policy, Roles = "reporting.edit")]
        [HttpPut(PutTaskRoute)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public ContentResult UpdateTask(int id, [FromBody] ApiTask task)
        {
            try
            {
                if (id != task.Id)
                    return new ContentResult
                    {
                        Content = "Request id does not match task id",
                        StatusCode = StatusCodes.Status400BadRequest
                    };

                logic.UpdateTask(mapper.Map<DtoTask>(task), task.BindedOpers);

                return new ContentResult {StatusCode = StatusCodes.Status200OK};
            }
            catch
            {
                return GetInternalErrorResult();
            }
        }
    }
}