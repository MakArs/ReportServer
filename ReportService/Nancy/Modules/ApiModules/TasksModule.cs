using System.Collections.Generic;
using Nancy;
using System.Threading.Tasks;
using Nancy.ModelBinding;
using Nancy.Security;
using Nancy.Swagger.Annotations.Attributes;
using ReportService.Entities;
using ReportService.Interfaces.Core;
using ReportService.Nancy.Models;
using Swagger.ObjectModel;
using Response = Nancy.Response;

namespace ReportService.Nancy.Modules.ApiModules
{
    public sealed class TasksModule : NancyBaseModule
    {
        public const string GetTasksRoute = "/api/v2/tasks";
        public const string StopTaskRoute = "/api/v2/tasks/stop/{taskinstanceid:long}";
        public const string GetTaskInstancesRoute = "/api/v2/tasks/{taskid}/instances";
        public const string GetCurrentTaskViewRoute = "/api/v2/tasks/{taskid}/currentviews";
        public const string RunTasksRoute = "/api/v2/tasks/run/{id}";
        public const string GetWorkingTaskInstancesRoute = "/api/v2/tasks/working-{taskId}";
        public const string DeleteTaskRoute = "/api/v2/tasks/{id}";
        public const string PostTaskRoute = "/api/v2/tasks";
        public const string PutTaskRoute = "/api/v2/tasks/{id}";

        [Route(nameof(GetAllTasks))]
        [Route(HttpMethod.Get, GetTasksRoute)]
        [Route(
            Tags = new[] {"Tasks"},
            Summary = "Method for receiving all tasks in service")]
        [RouteParam(
            ParamIn = ParameterIn.Header,
            Name = "Authorization",
            ParamType = typeof(string),
            Required = true,
            Description = "JWT access token")]
        [SwaggerResponse(
            HttpStatusCode.OK,
            Message = "Success",
            Model = typeof(List<DtoTask>))]
        [SwaggerResponse(
            HttpStatusCode.InternalServerError,
            "Internal error during request execution")]
        public Response GetAllTasks(ILogic logic)
        {
            try
            {
                var response = (Response) logic.GetAllTasksJson();

                response.StatusCode = HttpStatusCode.OK;
                return response;
            }
            catch
            {
                return HttpStatusCode.InternalServerError;
            }
        }

        [Route(nameof(StopInstanceAsync))]
        [Route(HttpMethod.Get, StopTaskRoute)]
        [Route(
            Tags = new[] {"Tasks"},
            Summary = "Method for stop working instance of task by ID of instance")]
        [RouteParam(
            ParamIn = ParameterIn.Path,
            Name = "taskinstanceid",
            ParamType = typeof(long),
            Required = true,
            Description = "Id of task instance that you need to stop")]
        [RouteParam(
            ParamIn = ParameterIn.Header,
            Name = "Authorization",
            ParamType = typeof(string),
            Required = true,
            Description = "JWT access token")]
        [SwaggerResponse(
            HttpStatusCode.OK,
            Message = "Success",
            Model = typeof(bool))]
        [SwaggerResponse(
            HttpStatusCode.InternalServerError,
            "Internal error during request execution")]
        public async Task<Response> StopInstanceAsync(ILogic logic)
        {
            this.RequiresClaims(c => c.Type == PermissionsType
                                     && (c.Value.Contains(StopRunPermission)
                                         || c.Value.Contains(EditPermission)));

            try
            {
                var stopped = await logic.StopTaskByInstanceIdAsync(Context.Parameters.taskinstanceid);

                var response = (Response) stopped.ToString();
                response.StatusCode = HttpStatusCode.OK;
                return response;
            }
            catch
            {
                return HttpStatusCode.InternalServerError;
            }
        }

        [Route(nameof(GetTaskInstances))]
        [Route(HttpMethod.Get, GetTaskInstancesRoute)]
        [Route(
            Tags = new[] {"Tasks"},
            Summary = "Method for receiving all instances of task by task ID")]
        [RouteParam(
            ParamIn = ParameterIn.Path,
            Name = "taskid",
            ParamType = typeof(int),
            Required = true,
            Description = "Id of task which instances you are need")]
        [RouteParam(
            ParamIn = ParameterIn.Header,
            Name = "Authorization",
            ParamType = typeof(string),
            Required = true,
            Description = "JWT access token")]
        [SwaggerResponse(
            HttpStatusCode.OK,
            Message = "Success",
            Model = typeof(List<DtoTaskInstance>))]
        [SwaggerResponse(
            HttpStatusCode.InternalServerError,
            "Internal error during request execution")]
        public Response GetTaskInstances(ILogic logic)
        {
            try
            {
                var response =
                    (Response) logic.GetAllTaskInstancesByTaskIdJson(Context.Parameters.taskid);
                response.StatusCode = HttpStatusCode.OK;
                return response;
            }
            catch
            {
                return HttpStatusCode.InternalServerError;
            }
        }

        [Route(nameof(GetTaskViewAsync))] //todo: make without exporters executing
        [Route(HttpMethod.Get, GetCurrentTaskViewRoute)]
        [Route(
            Tags = new[] {"Tasks"},
            Summary = "Method for executing task and open last dataset as html in browser by task ID")]
        [RouteParam(
            ParamIn = ParameterIn.Path,
            Name = "taskid",
            ParamType = typeof(int),
            Required = true,
            Description = "Id of task which view you are need")]
        [RouteParam(
            ParamIn = ParameterIn.Header,
            Name = "Authorization",
            ParamType = typeof(string),
            Required = true,
            Description = "JWT access token")]
        [SwaggerResponse(
            HttpStatusCode.OK,
            Message = "Success",
            Model = typeof(string))]
        [SwaggerResponse(
            HttpStatusCode.InternalServerError,
            "Internal error during request execution")]
        public async Task<Response> GetTaskViewAsync(ILogic logic)
        {
            try
            {
                var response = (Response) await logic
                    .GetCurrentViewByTaskIdAsync(Context.Parameters.taskid);
                response.StatusCode = HttpStatusCode.OK;
                return response;
            }
            catch
            {
                return HttpStatusCode.InternalServerError;
            }
        }

        [Route(nameof(RunTask))]
        [Route(HttpMethod.Get, RunTasksRoute)]
        [Route(Tags = new[] {"Tasks"},
            Summary = "Method for manual task executing")]
        [RouteParam(
            ParamIn = ParameterIn.Path,
            Name = "id",
            ParamType = typeof(int),
            Required = true,
            Description = "Id of task you need to execute")]
        [RouteParam(
            ParamIn = ParameterIn.Header,
            Name = "Authorization",
            ParamType = typeof(string),
            Required = true,
            Description = "JWT access token")]
        [SwaggerResponse(
            HttpStatusCode.OK,
            Message = "Success",
            Model = typeof(string))]
        [SwaggerResponse(
            HttpStatusCode.InternalServerError,
            "Internal error during request execution")]
        public Response RunTask(ILogic logic)
        {
            this.RequiresClaims(c => c.Type == PermissionsType
                                     && (c.Value.Contains(StopRunPermission)
                                         || c.Value.Contains(EditPermission)));

            try
            {
                string sentReps = logic.ForceExecute(Context.Parameters.id);
                var response = (Response) sentReps;
                response.StatusCode = HttpStatusCode.OK;
                return response;
            }

            catch
            {
                return HttpStatusCode.InternalServerError;
            }
        }

        [Route(nameof(GetWorkingTaskInstances))]
        [Route(HttpMethod.Get, GetWorkingTaskInstancesRoute)]
        [Route(
            Tags = new[] {"Tasks"},
            Summary = "Method for receiving IDs of working task instances by task ID")]
        [RouteParam(
            ParamIn = ParameterIn.Path,
            Name = "taskid",
            ParamType = typeof(int),
            Required = true,
            Description = "Id of task which instances you are need")]
        [RouteParam(
            ParamIn = ParameterIn.Header,
            Name = "Authorization",
            ParamType = typeof(string),
            Required = true,
            Description = "JWT access token")]
        [SwaggerResponse(
            HttpStatusCode.OK,
            Message = "Success",
            Model = typeof(List<DtoTaskInstance>))]
        [SwaggerResponse(
            HttpStatusCode.InternalServerError,
            "Internal error during request execution")]
        public Response GetWorkingTaskInstances(ILogic logic)
        {
            try
            {
                string sentReps = logic.GetWorkingTasksByIdJson(Context.Parameters.taskId);
                var response = (Response) sentReps;
                response.StatusCode = HttpStatusCode.OK;
                return response;
            }

            catch
            {
                return HttpStatusCode.InternalServerError;
            }
        }

        [Route(nameof(DeleteTask))]
        [Route(HttpMethod.Delete, DeleteTaskRoute)]
        [Route(
            Tags = new[] {"Tasks"},
            Summary = "Method for deleting task by ID")]
        [RouteParam(
            ParamIn = ParameterIn.Path,
            Name = "id",
            ParamType = typeof(int),
            Required = true,
            Description = "Id of task that you need to delete")]
        [RouteParam(
            ParamIn = ParameterIn.Header,
            Name = "Authorization",
            ParamType = typeof(string),
            Required = true,
            Description = "JWT access token")]
        [SwaggerResponse(HttpStatusCode.OK, Message = "Success")]
        [SwaggerResponse(
            HttpStatusCode.InternalServerError,
            "Internal error during request execution")]
        public Response DeleteTask(ILogic logic)
        {
            this.RequiresClaims(c => c.Type == PermissionsType
                                     && c.Value.Contains(EditPermission));

            try
            {
                logic.DeleteTask(Context.Parameters.id);
                return HttpStatusCode.OK;
            }
            catch
            {
                return HttpStatusCode.InternalServerError;
            }
        }

        [Route(nameof(CreateTask))]
        [Route(HttpMethod.Post, PostTaskRoute)]
        [Route(Consumes = new[] {"application/json"})]
        [Route(
            Tags = new[] {"Tasks"},
            Summary = "Method for creating task")]
        [RouteParam(
            ParamIn = ParameterIn.Body,
            Name = "task",
            ParamType = typeof(DtoSchedule),
            Required = true,
            Description = "New task")]
        [RouteParam(
            ParamIn = ParameterIn.Header,
            Name = "Authorization",
            ParamType = typeof(string),
            Required = true,
            Description = "JWT access token")]
        [SwaggerResponse(
            HttpStatusCode.OK,
            Message = "Success",
            Model = typeof(int))]
        [SwaggerResponse(
            HttpStatusCode.InternalServerError,
            "Internal error during request execution")]
        public Response CreateTask(ILogic logic)
        {
            this.RequiresClaims(c => c.Type == PermissionsType
                                     && c.Value.Contains(EditPermission));

            try
            {
                var newTask = this.Bind<ApiTask>();
                var id = logic.CreateTask(newTask);
                var response = (Response) $"{id}";
                response.StatusCode = HttpStatusCode.OK;
                return response;
            }
            catch
            {
                return HttpStatusCode.InternalServerError;
            }
        }

        [Route(nameof(UpdateTask))]
        [Route(HttpMethod.Put, PutTaskRoute)]
        [Route(Consumes = new[] {"application/json"})]
        [Route(
            Tags = new[] {"Tasks"},
            Summary = "Method for updating task")]
        [RouteParam(
            ParamIn = ParameterIn.Body,
            Name = "task",
            ParamType = typeof(DtoSchedule),
            Required = true,
            Description = "Existing task")]
        [RouteParam(
            ParamIn = ParameterIn.Path,
            Name = "id",
            ParamType = typeof(int),
            Required = true,
            Description = "Id of task that you need to update")]
        [RouteParam(
            ParamIn = ParameterIn.Header,
            Name = "Authorization",
            ParamType = typeof(string),
            Required = true,
            Description = "JWT access token")]
        [SwaggerResponse(HttpStatusCode.OK, Message = "Success")]
        [SwaggerResponse(
            HttpStatusCode.BadRequest,
            "Post query id does not matches with query body id")]
        [SwaggerResponse(
            HttpStatusCode.InternalServerError,
            "Internal error during request execution")]
        public Response UpdateTask(ILogic logic)
        {
            this.RequiresClaims(c => c.Type == PermissionsType
                                     && c.Value.Contains(EditPermission));

            try
            {
                var existingTask = this.Bind<ApiTask>
                    (new BindingConfig {BodyOnly = true});

                if (int.Parse(Context.Parameters.id) != existingTask.Id)
                    return HttpStatusCode.BadRequest;

                logic.UpdateTask(existingTask);
                return HttpStatusCode.OK;
            }
            catch
            {
                return HttpStatusCode.InternalServerError;
            }
        }

        public TasksModule(ILogic logic)
        {
            this.RequiresClaims(c => c.Type == PermissionsType
                                     && (c.Value.Contains(ViewPermission)
                                         || c.Value.Contains(StopRunPermission)
                                         || c.Value.Contains(EditPermission)));

            Get(GetTasksRoute,
                _ => GetAllTasks(logic),
                name: nameof(GetAllTasks));

            Get(StopTaskRoute,
                async (parameters, _) => await StopInstanceAsync(logic),
                name: nameof(StopInstanceAsync));

            Get(GetTaskInstancesRoute,
                parameters => GetTaskInstances(logic),
                name: nameof(GetTaskInstancesRoute));

            Get(GetCurrentTaskViewRoute,
                async (parameters, _) => await GetTaskViewAsync(logic),
                name: nameof(GetTaskViewAsync));

            Get(RunTasksRoute,
                parameters => RunTask(logic),
                name: nameof(RunTask));

            Get(GetWorkingTaskInstancesRoute,
                parameters => GetWorkingTaskInstances(logic),
                name: nameof(GetWorkingTaskInstances));

            Delete(DeleteTaskRoute,
                parameters => DeleteTask(logic),
                name: nameof(DeleteTask));

            Post(PostTaskRoute,
                parameters => CreateTask(logic),
                name: nameof(CreateTask));

            Put(PutTaskRoute,
                parameters => UpdateTask(logic),
                name: nameof(UpdateTask));
        }
    }
}