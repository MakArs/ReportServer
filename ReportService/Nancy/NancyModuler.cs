using System;
using Nancy;
using Nancy.ModelBinding;
using ReportService.Interfaces;
using ReportService.Nancy;

namespace ReportService.Implementations
{

    public class ReportsModule : NancyBaseModule
    {
        public ReportsModule(ILogic logic)
        {
            Get["/reports.html"] = parameters =>
            {
                try
                {
                    var response = (Response) $"{logic.GetTaskList_HtmlPage()}";
                    response.StatusCode = HttpStatusCode.OK;
                    return response;
                }
                catch
                {
                    return HttpStatusCode.InternalServerError;
                }
            };

            Get["/reports-{id:int}.html"] = parameters =>
            {
                try
                {
                    var response = (Response) $"{logic.GetInstanceList_HtmlPage(parameters.id)}";
                    response.StatusCode = HttpStatusCode.OK;
                    return response;
                }
                catch
                {
                    return HttpStatusCode.InternalServerError;
                }
            };

            Get["/send"] = parameters =>
            {
                int id = Request.Query.id;
                string mail = Request.Query.address;
                try
                {
                    string sentReps = logic.ForceExecute(id, mail);
                    var response = sentReps != ""
                        ? (Response) $"Reports {sentReps} sent!"
                        : "No reports with this id found...";
                    response.StatusCode = HttpStatusCode.OK;
                    return response;
                }
                catch
                {
                    return HttpStatusCode.InternalServerError;
                }
            };

            Post["/databases"] = parameters =>
            {
                try
                {
                    logic.CreateBase(parameters.ConnectionString);
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
}
