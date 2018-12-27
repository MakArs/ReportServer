using Nancy;
using ReportService.Interfaces.Core;

namespace ReportService.Nancy
{
    public class SiteModule : NancyBaseModule
    {
        public SiteModule(ILogic logic)
        {
            // this.RequiresClaims();
            ModulePath = "/site";

            Get["/tasks.html", runAsync: true] = async (parameters, token) =>
            {
                try
                {
                    var response = (Response) $"{await logic.GetTasksList_HtmlPage()}";
                    response.StatusCode = HttpStatusCode.OK;
                    return response;
                }
                catch
                {
                    return HttpStatusCode.InternalServerError;
                }
            };

            Get["/tasks-{id:int}.html", runAsync: true] = async (parameters, token) =>
            {
                try
                {
                    var response = (Response) $@"{await logic
                        .GetFullInstanceList_HtmlPage(parameters.id)}";
                    response.StatusCode = HttpStatusCode.OK;
                    return response;
                }
                catch
                {
                    return HttpStatusCode.InternalServerError;
                }
            };

            Get["/sendto"] = parameters =>
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
            };

            Get["/tasks/inwork.html", runAsync: true] = async (parameters, token) =>
            {
                try
                {
                    string entities = await logic.GetTasksInWorkList_HtmlPage();
                    var response = (Response) entities;
                    response.StatusCode = HttpStatusCode.OK;
                    return response;
                }
                catch
                {
                    return HttpStatusCode.InternalServerError;
                }
            };

            Get["/entities"] = parameters =>
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
            };

            Get["/run-{id:int}/confirm"] = parameters =>
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
            };
        }
    } //class
}