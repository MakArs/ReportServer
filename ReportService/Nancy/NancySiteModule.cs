using Nancy;
using ReportService.Interfaces;

namespace ReportService.Nancy
{
    public class SiteModule : NancyBaseModule
    {
        public SiteModule(ILogic logic)
        {
            ModulePath = "/site";

            Get["/tasks.html"] = parameters =>
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

            Get["/tasks-{id:int}.html"] = parameters =>
            {
                try
                {
                    var response = (Response) $"{logic.GetFullInstanceList_HtmlPage(parameters.id)}";
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
                int    id   = Request.Query.id;
                string mail = Request.Query.address;
                try
                {
                    string sentReps = logic.ForceExecute(id, mail);
                    var    response = (Response) sentReps;
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
