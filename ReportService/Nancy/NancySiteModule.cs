using Nancy;
using ReportService.Interfaces.Core;

namespace ReportService.Nancy
{
    public sealed class SiteModule : NancyBaseModule
    {
        public SiteModule(ILogic logic)
        {
            // this.RequiresClaims();
            ModulePath = "/site";

            Get("/tasks.html", async (parameters, token) =>
            {
                try
                {
                    var response = (Response) $"{await logic.GetTasksList_HtmlPageAsync()}";
                    response.StatusCode = HttpStatusCode.OK;
                    return response;
                }
                catch
                {
                    return HttpStatusCode.InternalServerError;
                }
            },name: "GetAllTasksHtml");

            Get("/tasks-{id:int}.html",  async (parameters, token) =>
            {
                try
                {
                    var response = (Response) $@"{await logic
                        .GetFullInstanceList_HtmlPageAsync(parameters.id)}";
                    response.StatusCode = HttpStatusCode.OK;
                    return response;
                }
                catch
                {
                    return HttpStatusCode.InternalServerError;
                }
            }, name: "GetAllTaskInstancesHtml");

            Get("/sendto", parameters =>
            {
                int id = Request.Query.id;
                string mail = Request.Query.address;
                try
                {
                    string sentReps = logic.SendDefault(id, mail);
                    var response = (Response) sentReps;
                    response.StatusCode = HttpStatusCode.OK;
                    return response;
                }
                catch
                {
                    return HttpStatusCode.InternalServerError;
                }
            }, name: "RunTasksToEmail");

            Get("/tasks/inwork.html", async (parameters, token) =>
            {
                try
                {
                    string entities = await logic.GetTasksInWorkList_HtmlPageAsync();
                    var response = (Response) entities;
                    response.StatusCode = HttpStatusCode.OK;
                    return response;
                }
                catch
                {
                    return HttpStatusCode.InternalServerError;
                }
            }, name: "GetAllInWorkTasksHtml");

            Get("/entities", parameters =>
            {
                try
                {
                    string entities = logic.GetEntitiesCountJson();
                    var response = (Response) entities;
                    response.StatusCode = HttpStatusCode.OK;
                    return response;
                }
                catch
                {
                    return HttpStatusCode.InternalServerError;
                }
            }, name: "GetAllEntitiesHtml");

            Get("/run-{id:int}/confirm", parameters =>
            {
                try
                {
                    string sentReps = logic.ForceExecute(parameters.id);
                    var response = (Response) sentReps;
                    response.StatusCode = HttpStatusCode.OK;
                    return response;
                }
                catch
                {
                    return HttpStatusCode.InternalServerError;
                }
            }, name: "RunTask");
        }
    } //class
}