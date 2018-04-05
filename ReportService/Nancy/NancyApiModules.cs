using System;
using System.Configuration;
using Nancy;
using Nancy.ModelBinding;
using ReportService.Interfaces;

namespace ReportService.Nancy
{
    public class DataBaseModule : NancyBaseModule
    {
        public DataBaseModule(ILogic logic)
        {
            ModulePath = "/api/v1";

            Post["/databases"] = parameters =>
            {
                try
                {
                    logic.CreateBase(ConfigurationManager.AppSettings["DBConnStr"]);
                    var response = (Response) "DataBase successfully created!";
                    response.StatusCode = HttpStatusCode.OK;
                    return response;
                }
                catch (Exception e)
                {
                    var response = (Response) $"DataBase was not created...{e.Message}";
                    response.StatusCode = HttpStatusCode.InternalServerError;
                    return response;
                }
            };
        }
    } //class

    public class ReportsModule : NancyBaseModule
    {
        public ReportsModule(ILogic logic)
        {
            ModulePath = "/api/v1";

            Get["/reports"] = parameters =>
            {
                try
                {
                    var response = (Response) logic.GetAllTaskCompacts();

                    response.StatusCode = HttpStatusCode.OK;
                    return response;
                }
                catch 
                {
                    return HttpStatusCode.InternalServerError;
                }
            };

            Get["/reports/{id:int}"] = parameters =>
            {
                try
                {
                    var response = (Response) logic.GetTaskById(parameters.id);
                    response.StatusCode = HttpStatusCode.OK;
                    return response;
                }
                catch
                {
                    return HttpStatusCode.InternalServerError;
                }
            };

            Delete["/reports/{id:int}"] = parameters =>
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

            Post["/reports"] = parameters =>
            {
                try
                {
                    var newTask = this.Bind<ApiTask>();
                    var id = logic.CreateTask(newTask);
                    var response = (Response) $"created task {id}";
                    response.StatusCode = HttpStatusCode.OK;
                    return response;
                }
                catch
                {
                    return HttpStatusCode.InternalServerError;
                }
            };

            Put["/reports/{id:int}"] = parameters =>
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

            Get["/reports/{reportid:int}/instances"] = parameters =>
            {
                try
                {
                    var response = (Response) logic.GetAllInstanceCompactsByTaskIdJson(parameters.reportid);
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
                    var response = (Response) logic.GetAllInstancesCompactJson();
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
                    var response = (Response) logic.GetInstanceByIdJson(parameters.id);
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
}

