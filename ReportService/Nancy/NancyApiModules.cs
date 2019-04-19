using System.Linq;
using Nancy;
using Nancy.ModelBinding;
using Nancy.Security;
using Nancy.Swagger.Annotations.Attributes;
using Newtonsoft.Json;
using ReportService.Interfaces.Core;
using Swagger.ObjectModel;
using Response = Nancy.Response;

namespace ReportService.Nancy
{
    public sealed class GeneralModule : NancyBaseModule
    {
        public const string IsAliveRoute = "/api/v2";
        public const string GetRoleRoute = "/api/v2/roles";

        [Route(nameof(IsAlive))]
        [Route(HttpMethod.Get, IsAliveRoute)]
        [Route(Tags = new[] { "General" },Summary = "Method for checking if service is alive")]
        [SwaggerResponse(HttpStatusCode.OK, Message = "Success")]
        public object IsAlive()
        {
            try
            {
                var response = new Response {StatusCode = HttpStatusCode.OK};
                return response;
            }
            catch
            {
                return HttpStatusCode.InternalServerError;
            }
        }

        [Route(nameof(GetUserRole))]
        [Route(HttpMethod.Get, GetRoleRoute)]
        [Route(Tags = new[] { "General" }, Summary = "Method for getting role of request user")]
        [SwaggerResponse(HttpStatusCode.OK, Message = "Success")]
        public Response GetUserRole()
        {
            try
            {
                var claims = Context.CurrentUser.Claims
                    .FirstOrDefault(claim => claim.Type == PermissionsType)?.Value;

                ApiUserRole role = ApiUserRole.NoRole;

                if (claims != null)
                    if (claims.Contains(EditPermission))
                        role = ApiUserRole.Editor;
                    else if (claims.Contains(StopRunPermission))
                        role = ApiUserRole.StopRunner;
                    else if (claims.Contains(ViewPermission))
                        role = ApiUserRole.Viewer;

                var response = (Response) JsonConvert.SerializeObject(role);
                response.StatusCode = HttpStatusCode.OK;
                return response;
            }
            catch
            {
                return HttpStatusCode.InternalServerError;
            }
        }

        public GeneralModule()
        {
            Get(IsAliveRoute,
                _ => IsAlive(),
                name: nameof(IsAlive));

            Get(GetRoleRoute, _ =>
                GetUserRole(), name: nameof(GetUserRole));

        }
    }

    public sealed class OpersModule : NancyBaseModule
    {
        public OpersModule(ILogic logic)
        {
            //can do through RequiresAnyClaim but more code
            this.RequiresClaims(c => c.Type == PermissionsType
                                     && (c.Value.Contains(ViewPermission)
                                         || c.Value.Contains(StopRunPermission)
                                         || c.Value.Contains(EditPermission)));

            ModulePath = "/api/v2/opertemplates";

            Get("/", parameters =>
            {
                try
                {
                    var response = (Response)logic.GetAllOperTemplatesJson();
                    response.StatusCode = HttpStatusCode.OK;
                    return response;
                }
                catch
                {
                    return HttpStatusCode.InternalServerError;
                }
            }, name: "GetTemplates");

            Get("/registeredimporters", parameters =>
            {
                try
                {
                    var response = (Response)logic.GetAllRegisteredImportersJson();
                    response.StatusCode = HttpStatusCode.OK;
                    return response;
                }
                catch
                {
                    return HttpStatusCode.InternalServerError;
                }
            }, name: "GetImporters");

            Get("/registeredexporters", parameters =>
            {
                try
                {
                    var response = (Response)logic.GetAllRegisteredExportersJson();
                    response.StatusCode = HttpStatusCode.OK;
                    return response;
                }
                catch
                {
                    return HttpStatusCode.InternalServerError;
                }
            }, name: "GetExporters");

            Get("/taskopers", parameters =>
            {
                try
                {
                    var response = (Response)logic.GetAllOperationsJson();
                    response.StatusCode = HttpStatusCode.OK;
                    return response;
                }
                catch
                {
                    return HttpStatusCode.InternalServerError;
                }
            }, name: "GetTaskOpers");

            Delete("/{id:int}", parameters =>
            {
                this.RequiresClaims(c => c.Type == PermissionsType
                                         && c.Value.Contains(EditPermission));
                try
                {
                    logic.DeleteOperationTemplate(parameters.id);
                    return HttpStatusCode.OK;
                }
                catch
                {
                    return HttpStatusCode.InternalServerError;
                }
            }, name: "DeleteTemplate");

            Post("/", parameters =>
            {
                this.RequiresClaims(c => c.Type == PermissionsType
                                         && c.Value.Contains(EditPermission));
                try
                {
                    var newOper = this.Bind<DtoOperTemplate>();
                    var id = logic.CreateOperationTemplate(newOper);
                    var response = (Response)$"{id}";
                    response.StatusCode = HttpStatusCode.OK;
                    return response;
                }
                catch
                {
                    return HttpStatusCode.InternalServerError;
                }
            }, name: "CreateTemplate");

            Put("/{id:int}", parameters =>
            {
                this.RequiresClaims(c => c.Type == PermissionsType
                                         && c.Value.Contains(EditPermission));
                try
                {
                    var existingOper = this.Bind<DtoOperTemplate>
                        (new BindingConfig { BodyOnly = true });

                    if (parameters.id != existingOper.Id)
                        return HttpStatusCode.BadRequest;

                    logic.UpdateOperationTemplate(existingOper);
                    return HttpStatusCode.OK;
                }
                catch
                {
                    return HttpStatusCode.InternalServerError;
                }
            }, name: "UpdateTemplate");
        }
    } //OperTemplates&Operations Module

    public sealed class RecepientGroupsModule : NancyBaseModule
    {
        public RecepientGroupsModule(ILogic logic)
        {
            this.RequiresAnyClaim(c => c.Type == PermissionsType
                                       && (c.Value.Contains(ViewPermission)
                                           || c.Value.Contains(StopRunPermission)
                                           || c.Value.Contains(EditPermission)));

            ModulePath = "/api/v2/recepientgroups";

            Get("/", parameters =>
            {
                try
                {
                    var response = (Response)logic.GetAllRecepientGroupsJson();
                    response.StatusCode = HttpStatusCode.OK;
                    return response;
                }
                catch
                {
                    return HttpStatusCode.InternalServerError;
                }
            }, name: "GetRecepients");

            Delete("/{id:int}", parameters =>
            {
                this.RequiresClaims(c => c.Type == PermissionsType
                                         && c.Value.Contains(EditPermission));
                try
                {
                    logic.DeleteRecepientGroup(parameters.id);
                    return HttpStatusCode.OK;
                }
                catch
                {
                    return HttpStatusCode.InternalServerError;
                }
            }, name: "DeleteRecepientGroup");

            Post("/", parameters =>
            {
                this.RequiresClaims(c => c.Type == PermissionsType
                                         && c.Value.Contains(EditPermission));
                try
                {
                    var newReport = this.Bind<DtoRecepientGroup>();
                    var id = logic.CreateRecepientGroup(newReport);
                    var response = (Response)$"{id}";
                    response.StatusCode = HttpStatusCode.OK;
                    return response;
                }
                catch
                {
                    return HttpStatusCode.InternalServerError;
                }
            }, name: "CreateRecepientGroup");


            Put("/{id:int}", parameters =>
            {
                this.RequiresClaims(c => c.Type == PermissionsType
                                         && c.Value.Contains(EditPermission));

                try
                {
                    var existingGroup = this.Bind<DtoRecepientGroup>
                        (new BindingConfig { BodyOnly = true });

                    if (parameters.id != existingGroup.Id)
                        return HttpStatusCode.BadRequest;

                    logic.UpdateRecepientGroup(existingGroup);
                    return HttpStatusCode.OK;
                }
                catch
                {
                    return HttpStatusCode.InternalServerError;
                }
            }, name: "UpdateRecepientGroup");
        }
    } //RecepientGroupsModule

    public sealed class TelegramModule : NancyBaseModule
    {
        public TelegramModule(ILogic logic)
        {

            this.RequiresAnyClaim(c => c.Type == PermissionsType
                                       && (c.Value.Contains(ViewPermission)
                                           || c.Value.Contains(StopRunPermission)
                                           || c.Value.Contains(EditPermission)));

            ModulePath = "/api/v2/telegrams";

            Get("/", parameters =>
            {
                try
                {
                    var response = (Response)logic.GetAllTelegramChannelsJson();
                    response.StatusCode = HttpStatusCode.OK;
                    return response;
                }
                catch
                {
                    return HttpStatusCode.InternalServerError;
                }
            }, name: "GetTgChannels");

            Post("/", parameters =>
            {
                this.RequiresClaims(c => c.Type == PermissionsType
                                         && c.Value.Contains(EditPermission));
                try
                {
                    var newTelegramChannel = this.Bind<DtoTelegramChannel>();
                    var id = logic.CreateTelegramChannel(newTelegramChannel);
                    var response = (Response)$"{id}";
                    response.StatusCode = HttpStatusCode.OK;
                    return response;
                }
                catch
                {
                    return HttpStatusCode.InternalServerError;
                }
            }, name: "CreateTgChannel");

            Put("/{id:int}", parameters =>
            {
                this.RequiresClaims(c => c.Type == PermissionsType
                                         && c.Value.Contains(EditPermission));
                try
                {
                    var existingTelegramChannel = this.Bind<DtoTelegramChannel>
                        (new BindingConfig { BodyOnly = true });

                    if (parameters.id != existingTelegramChannel.Id)
                        return HttpStatusCode.BadRequest;

                    logic.UpdateTelegramChannel(existingTelegramChannel);
                    return HttpStatusCode.OK;
                }
                catch
                {
                    return HttpStatusCode.InternalServerError;
                }
            }, name: "UpdateTgChannel");

        }
    } //TelegramModule

    public sealed class SyncModule : NancyBaseModule
    {
        public SyncModule(ILogic logic)
        {
            ModulePath = "/api/v2/synchronizations";

            //Get["/currentexporters"] = parameters =>
            //{
            //    try
            //    {
            //        var response = (Response) logic.GetAllTelegramChannelsJson();
            //        response.StatusCode = HttpStatusCode.OK;
            //        return response;
            //    }
            //    catch
            //    {
            //        return HttpStatusCode.InternalServerError;
            //    }
            //};

            Post("/tasks", parameters =>
            {
                this.RequiresClaims(c => c.Type == PermissionsType
                                         && c.Value.Contains(EditPermission));

                try
                {
                    var apiTask = this.Bind<ApiTask>();
                    var id = logic.CreateTask(apiTask);
                    var response = (Response)$"{id}";
                    response.StatusCode = HttpStatusCode.OK;
                    return response;
                }
                catch
                {
                    return HttpStatusCode.InternalServerError;
                }
            }, name: "SyncNewTask");
        }
    } //TelegramModule

    public sealed class ScheduleModule : NancyBaseModule
    {
        public ScheduleModule(ILogic logic)
        {
            this.RequiresClaims(c => c.Type == PermissionsType
                                     && (c.Value.Contains(ViewPermission)
                                         || c.Value.Contains(StopRunPermission)
                                         || c.Value.Contains(EditPermission)));

            ModulePath = "/api/v2/schedules";

            Get("/", parameters =>
            {
                try
                {
                    var response = (Response)logic.GetAllSchedulesJson();
                    response.StatusCode = HttpStatusCode.OK;
                    return response;
                }
                catch
                {
                    return HttpStatusCode.InternalServerError;
                }
            }, name: "GetSchedules");

            Delete("/{id:int}", parameters =>
            {
                this.RequiresClaims(c => c.Type == PermissionsType
                                         && c.Value.Contains(EditPermission));

                try
                {
                    logic.DeleteSchedule(parameters.id);
                    return HttpStatusCode.OK;
                }
                catch
                {
                    return HttpStatusCode.InternalServerError;
                }
            }, name: "DeleteSchedule");

            Post("/", parameters =>
            {
                this.RequiresClaims(c => c.Type == PermissionsType
                                         && c.Value.Contains(EditPermission));

                try
                {
                    var newSchedule = this.Bind<DtoSchedule>();
                    var id = logic.CreateSchedule(newSchedule);
                    var response = (Response)$"{id}";
                    response.StatusCode = HttpStatusCode.OK;
                    return response;
                }
                catch
                {
                    return HttpStatusCode.InternalServerError;
                }
            }, name: "CreateSchedule");


            Put("/{id:int}", parameters =>
            {
                this.RequiresClaims(c => c.Type == PermissionsType
                                         && c.Value.Contains(EditPermission));

                try
                {
                    var existingSchedule = this.Bind<DtoSchedule>
                        (new BindingConfig { BodyOnly = true });

                    if (parameters.id != existingSchedule.Id)
                        return HttpStatusCode.BadRequest;

                    logic.UpdateSchedule(existingSchedule);
                    return HttpStatusCode.OK;
                }
                catch
                {
                    return HttpStatusCode.InternalServerError;
                }
            }, name: "UpdateSchedule");

        }
    } //SchedulesModule

    public sealed class TasksModule : NancyBaseModule
    {
        public TasksModule(ILogic logic)
        {
            this.RequiresClaims(c => c.Type == PermissionsType
                                     && (c.Value.Contains(ViewPermission)
                                         || c.Value.Contains(StopRunPermission)
                                         || c.Value.Contains(EditPermission)));

            ModulePath = "/api/v2/tasks";

            Get("/", parameters =>
            {
                try
                {
                    var response = (Response)logic.GetAllTasksJson();

                    response.StatusCode = HttpStatusCode.OK;
                    return response;
                }
                catch
                {
                    return HttpStatusCode.InternalServerError;
                }
            }, name: "GetTasks");

            Get("/stop/{taskinstanceid:long}", async (parameters, _) =>
            {
                this.RequiresClaims(c => c.Type == PermissionsType
                                         && (c.Value.Contains(StopRunPermission)
                                             || c.Value.Contains(EditPermission)));

                try
                {
                    var stopped = await logic.StopTaskByInstanceIdAsync(parameters.taskinstanceid);

                    var response = (Response)stopped.ToString();
                    response.StatusCode = HttpStatusCode.OK;
                    return response;
                }
                catch
                {
                    return HttpStatusCode.InternalServerError;
                }
            }, name: "StopTask");

            Get("/{taskid:int}/instances", parameters =>
            {
                try
                {
                    var response =
                        (Response)logic.GetAllTaskInstancesByTaskIdJson(parameters.taskid);
                    response.StatusCode = HttpStatusCode.OK;
                    return response;
                }
                catch
                {
                    return HttpStatusCode.InternalServerError;
                }
            }, name: "GetTaskInstances");

            Get("/{taskid:int}/currentviews", async (parameters, _) =>
            {
                try
                {
                    var response = (Response)await logic.GetCurrentViewByTaskIdAsync(parameters.taskid);
                    response.StatusCode = HttpStatusCode.OK;
                    return response;
                }
                catch
                {
                    return HttpStatusCode.InternalServerError;
                }
            }, name: "GetCurrentTaskView");

            Get("/run-{id:int}", parameters =>
            {
                this.RequiresClaims(c => c.Type == PermissionsType
                                         && (c.Value.Contains(StopRunPermission)
                                             || c.Value.Contains(EditPermission)));

                try
                {
                    string sentReps = logic.ForceExecute(parameters.id);
                    var response = (Response)sentReps;
                    response.StatusCode = HttpStatusCode.OK;
                    return response;
                }

                catch
                {
                    return HttpStatusCode.InternalServerError;
                }
            }, name: "RunTask");

            Get("/working-{taskId:int}", parameters =>
            {
                try
                {
                    string sentReps = logic.GetWorkingTasksByIdJson(parameters.taskId);
                    var response = (Response)sentReps;
                    response.StatusCode = HttpStatusCode.OK;
                    return response;
                }

                catch
                {
                    return HttpStatusCode.InternalServerError;
                }
            }, name: "GetWorkingTask");

            Delete("/{id:int}", parameters =>
            {
                this.RequiresClaims(c => c.Type == PermissionsType
                                         && c.Value.Contains(EditPermission));

                try
                {
                    logic.DeleteTask(parameters.id);
                    return HttpStatusCode.OK;
                }
                catch
                {
                    return HttpStatusCode.InternalServerError;
                }
            }, name: "DeleteTask");

            Post("/", parameters =>
            {
                this.RequiresClaims(c => c.Type == PermissionsType
                                         && c.Value.Contains(EditPermission));

                try
                {
                    var newTask = this.Bind<ApiTask>();
                    var id = logic.CreateTask(newTask);
                    var response = (Response)$"{id}";
                    response.StatusCode = HttpStatusCode.OK;
                    return response;
                }
                catch
                {
                    return HttpStatusCode.InternalServerError;
                }
            }, name: "CreateTask");

            Put("/{id:int}", parameters =>
            {
                this.RequiresClaims(c => c.Type == PermissionsType
                                         && c.Value.Contains(EditPermission));

                try
                {
                    var existingTask = this.Bind<ApiTask>
                        (new BindingConfig { BodyOnly = true });

                    if (parameters.id != existingTask.Id)
                        return HttpStatusCode.BadRequest;

                    logic.UpdateTask(existingTask);
                    return HttpStatusCode.OK;
                }
                catch
                {
                    return HttpStatusCode.InternalServerError;
                }
            }, name: "UpdateTask");
        }
    } //TasksModule

    public sealed class InstancesModule : NancyBaseModule
    {
        public InstancesModule(ILogic logic)
        {
            this.RequiresClaims(c => c.Type == PermissionsType
                                     && (c.Value.Contains(ViewPermission)
                                         || c.Value.Contains(StopRunPermission)
                                         || c.Value.Contains(EditPermission)));

            ModulePath = "/api/v2/instances";

            Delete("/{id:int}", parameters =>
            {
                this.RequiresClaims(c => c.Type == PermissionsType
                                         && c.Value.Contains(EditPermission));

                try
                {
                    logic.DeleteTaskInstanceById(parameters.id);
                    return HttpStatusCode.OK;
                }
                catch
                {
                    return HttpStatusCode.InternalServerError;
                }
            }, name: "DeleteTaskInstance");

            // TODO: filter - top, paginations
            Get("/", parameters =>
            {
                try
                {
                    var response = (Response)logic.GetAllTaskInstancesJson();
                    response.StatusCode = HttpStatusCode.OK;
                    return response;
                }
                catch
                {
                    return HttpStatusCode.InternalServerError;
                }
            }, name: "GetAllTaskInstances");

            Get("/{instanceid:int}/operinstances", parameters =>
            {
                try
                {
                    var response = (Response)logic.GetOperInstancesByTaskInstanceIdJson(parameters.instanceid);
                    response.StatusCode = HttpStatusCode.OK;
                    return response;
                }
                catch
                {
                    return HttpStatusCode.InternalServerError;
                }
            }, name: "GetAllOperInstances");

            Get("/operinstances/{id:int}", parameters =>
            {
                try
                {
                    var response = (Response)logic.GetFullOperInstanceByIdJson(parameters.id);
                    response.StatusCode = HttpStatusCode.OK;
                    return response;
                }
                catch
                {
                    return HttpStatusCode.InternalServerError;
                }
            }, name: "GetFullOperInstance");
        }
    } //Instances&OperInstancesModule
}
