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

            Get["/{id:int}"] = parameters =>
            {
                try
                {
                    var response = (Response) logic.GetFullTaskByIdJson(parameters.id);
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
                    var existingTask = this.Bind<ApiTask>();

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

        }
    } //class

    public class InstancesModule : NancyBaseModule
    {
        public InstancesModule(ILogic logic)
        {
            ModulePath = "/api/v1";

            Delete["/instances/{id:int}"] = parameters =>
            {
                try
                {
                    logic.DeleteInstance(parameters.id);
                    return HttpStatusCode.OK;
                }
                catch
                {
                    return HttpStatusCode.InternalServerError;
                }
            };

            Get["/tasks/{taskid:int}/instances"] = parameters =>
            {
                try
                {
                    var response = (Response) logic.GetAllInstancesByTaskIdJson(parameters.taskid);
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
                    var response = (Response) logic.GetCurrentViewByTaskId(parameters.taskid);
                    response.StatusCode = HttpStatusCode.OK;
                    return response;
                }
                catch
                {
                    return HttpStatusCode.InternalServerError;
                }
            };

            // TODO: filter - top, paginations
            Get["/instances"] = parameters =>
            {
                try
                {
                    var response = (Response) logic.GetAllInstancesJson();
                    response.StatusCode = HttpStatusCode.OK;
                    return response;
                }
                catch
                {
                    return HttpStatusCode.InternalServerError;
                }
            };

            Get["/instances/{id:int}"] = parameters =>
            {
                try
                {
                    var response = (Response) logic.GetFullInstanceByIdJson(parameters.id);
                    response.StatusCode = HttpStatusCode.OK;
                    return response;
                }
                catch
                {
                    return HttpStatusCode.InternalServerError;
                }
            };
        }
    } //class

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
                    var existingSchedule = this.Bind<DtoSchedule>();

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
    } //class

    public class RecepientGroupsModule : NancyBaseModule
    {
        public RecepientGroupsModule(ILogic logic)
        {
            ModulePath = "/api/v1/recepientgroups";

            Get[""] = parameters =>
            {
                try
                {
                    var response = (Response) logic.GetAllExporterToTaskBindersJson();
                    response.StatusCode = HttpStatusCode.OK;
                    return response;
                }
                catch
                {
                    return HttpStatusCode.InternalServerError;
                }
            };

            //Post[""] = parameters =>
            //{
            //    try
            //    {
            //        var newReport = this.Bind<DtoRecepientGroup>();
            //        var id = logic.CreateRecepientGroup(newReport);
            //        var response = (Response)$"{id}";
            //        response.StatusCode = HttpStatusCode.OK;
            //        return response;
            //    }
            //    catch
            //    {
            //        return HttpStatusCode.InternalServerError;
            //    }
            //};


            //Put["/{id:int}"] = parameters =>
            //{
            //    try
            //    {
            //        var existingGroup = this.Bind<DtoRecepientGroup>();

            //        if (parameters.id != existingGroup.Id)
            //            return HttpStatusCode.BadRequest;

            //        logic.UpdateRecepientGroup(existingGroup);
            //        return HttpStatusCode.OK;
            //    }
            //    catch
            //    {
            //        return HttpStatusCode.InternalServerError;
            //    }
            //};
        }
    } //class

    public class ReportsModule : NancyBaseModule
    {
        public ReportsModule(ILogic logic)
        {
            ModulePath = "/api/v1/reports";

            Get[""] = parameters =>
            {
                try
                {
                    var response = (Response) logic.GetAllReportsJson();
                    response.StatusCode = HttpStatusCode.OK;
                    return response;
                }
                catch
                {
                    return HttpStatusCode.InternalServerError;
                }
            };

            Get["/customdataexecutors"] = parameters =>
            {
                try
                {
                    var response = (Response)logic.GetAllCustomDataExecutors();
                    response.StatusCode = HttpStatusCode.OK;
                    return response;
                }
                catch
                {
                    return HttpStatusCode.InternalServerError;
                }
            };

            Get["/customviewexecutors"] = parameters =>
            {
                try
                {
                    var response = (Response)logic.GetAllCustomViewExecutors();
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
                    var newReport = this.Bind<DtoReport>();
                    var id = logic.CreateReport(newReport);
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
                    var existingReport = this.Bind<DtoReport>();

                    if (parameters.id != existingReport.Id)
                        return HttpStatusCode.BadRequest;

                    logic.UpdateReport(existingReport);
                    return HttpStatusCode.OK;
                }
                catch
                {
                    return HttpStatusCode.InternalServerError;
                }
            };
        }
    } //class
}

