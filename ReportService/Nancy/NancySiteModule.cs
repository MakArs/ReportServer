using Nancy;
using ReportService.Interfaces;

namespace ReportService.Nancy
{
    public class SiteModule : NancyBaseModule
    {
        public SiteModule(ILogic logic)
        {
            ModulePath = "/site";

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
        }
    } //class
}
