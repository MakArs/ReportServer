using Nancy;
using Nancy.ModelBinding;
using ReportService.Interfaces;

namespace ReportService.Nancy
{
    public class TasksModule : NancyBaseModule
    {
        public TasksModule(ILogic logic)
        {
            ModulePath = "/api/v1/tasks";

            Get[""] = parameters =>
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
            };

            Delete["/{id:int}"] = parameters =>
            {
                try
                {
                    logic.DeleteTask(parameters.id);
                    return HttpStatusCode.OK;
                }
                catch
                {
                    return HttpStatusCode.InternalServerError;
                }
            };

            Post[""] = parameters =>
            {
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
            };

            Put["/{id:int}"] = parameters =>
            {
                try
                {
                    var existingTask = this.Bind<ApiTask>
                        (new BindingConfig{BodyOnly = true}); 

                    if (parameters.id != existingTask.Id)
                        return HttpStatusCode.BadRequest;

                    logic.UpdateTask(existingTask);
                    return HttpStatusCode.OK;
                }
                catch
                {
                    return HttpStatusCode.InternalServerError;
                }
            };

            Get["/{taskid:int}/instances"] = parameters =>
            {
                try
                {
                    var response = (Response)logic.GetAllTaskInstancesByTaskIdJson(parameters.taskid);
                    response.StatusCode = HttpStatusCode.OK;
                    return response;
                }
                catch
                {
                    return HttpStatusCode.InternalServerError;
                }
            };

            Get["/tasks/{taskid:int}/currentviews"] = parameters =>
            {
                try
                {
                    var response = (Response)logic.GetCurrentViewByTaskId(parameters.taskid);
                    response.StatusCode = HttpStatusCode.OK;
                    return response;
                }
                catch
                {
                    return HttpStatusCode.InternalServerError;
                }
            };
        }
    } //TasksModule

    public class InstancesModule : NancyBaseModule
    {
        public InstancesModule(ILogic logic)
        {
            ModulePath = "/api/v1/instances";

            Delete["/{id:int}"] = parameters =>
            {
                try
                {
                    logic.DeleteTaskInstanceById(parameters.id);
                    return HttpStatusCode.OK;
                }
                catch
                {
                    return HttpStatusCode.InternalServerError;
                }
            };

            // TODO: filter - top, paginations
            Get[""] = parameters =>
            {
                try
                {
                    var response = (Response) logic.GetAllTaskInstancesJson();
                    response.StatusCode = HttpStatusCode.OK;
                    return response;
                }
                catch
                {
                    return HttpStatusCode.InternalServerError;
                }
            };

            Get["/{instanceid:int}/operinstances"] = parameters =>
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
            };

            Get["/operinstances/{id:int}"] = parameters =>
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
            };
        }
    } //Instances&OperInstancesModule

    public class ScheduleModule : NancyBaseModule
    {
        public ScheduleModule(ILogic logic)
        {
            ModulePath = "/api/v1/schedules";

            Get[""] = parameters =>
            {
                try
                {
                    var response = (Response) logic.GetAllSchedulesJson();
                    response.StatusCode = HttpStatusCode.OK;
                    return response;
                }
                catch
                {
                    return HttpStatusCode.InternalServerError;
                }
            };

            Post[""] = parameters =>
            {
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
            };


            Put["/{id:int}"] = parameters =>
            {
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
            };

        }
    } //SchedulesModule

    public class TelegramModule : NancyBaseModule
    {
        public TelegramModule(ILogic logic)
        {
            ModulePath = "/api/v1/telegrams";

            Get[""] = parameters =>
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
            };

            Post[""] = parameters =>
            {
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
            };

            Put["/{id:int}"] = parameters =>
            {
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
            };

        }
    } //TelegramModule

    public class RecepientGroupsModule : NancyBaseModule
    {
        public RecepientGroupsModule(ILogic logic)
        {
            ModulePath = "/api/v1/recepientgroups";

            Get[""] = parameters =>
            {
                try
                {
                    var response = (Response) logic.GetAllRecepientGroupsJson();
                    response.StatusCode = HttpStatusCode.OK;
                    return response;
                }
                catch
                {
                    return HttpStatusCode.InternalServerError;
                }
            };

            Post[""] = parameters =>
            {
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
            };


            Put["/{id:int}"] = parameters =>
            {
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
            };
        }
    } //RecepientGroupsModule

    public class OpersModule : NancyBaseModule
    {
        public OpersModule(ILogic logic)
        {
            ModulePath = "/api/v1/opers";

            Get[""] = parameters =>
            {
                try
                {
                    var response = (Response) logic.GetAllOperationsJson();
                    response.StatusCode = HttpStatusCode.OK;
                    return response;
                }
                catch
                {
                    return HttpStatusCode.InternalServerError;
                }
            };

            Get["/customimporters"] = parameters =>
            {
                try
                {
                    var response = (Response)logic.GetAllCustomImporters();
                    response.StatusCode = HttpStatusCode.OK;
                    return response;
                }
                catch
                {
                    return HttpStatusCode.InternalServerError;
                }
            };

            Get["/customexporters"] = parameters =>
            {
                try
                {
                    var response = (Response)logic.GetAllCustomExporters();
                    response.StatusCode = HttpStatusCode.OK;
                    return response;
                }
                catch
                {
                    return HttpStatusCode.InternalServerError;
                }
            };

            Get["/taskopers"] = parameters =>
            {
                try
                {
                    var response = (Response)logic.GetAllTaskOpersJson();
                    response.StatusCode = HttpStatusCode.OK;
                    return response;
                }
                catch
                {
                    return HttpStatusCode.InternalServerError;
                }
            };

            Post[""] = parameters =>
            {
                try
                {
                    var newOper = this.Bind<DtoOper>();
                    var id = logic.CreateOperation(newOper);
                    var response = (Response) $"{id}";
                    response.StatusCode = HttpStatusCode.OK;
                    return response;
                }
                catch
                {
                    return HttpStatusCode.InternalServerError;
                }
            };

            Put["/{id:int}"] = parameters =>
            {
                try
                {
                    var existingOper = this.Bind<DtoOper>
                        (new BindingConfig { BodyOnly = true });

                    if (parameters.id != existingOper.Id)
                        return HttpStatusCode.BadRequest;

                    logic.UpdateOperation(existingOper);
                    return HttpStatusCode.OK;
                }
                catch
                {
                    return HttpStatusCode.InternalServerError;
                }
            };
        }
    } //Opers&TaskOpersModule
}

