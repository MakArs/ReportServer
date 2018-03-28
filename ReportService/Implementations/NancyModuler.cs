using System;
using Nancy;
using Nancy.ModelBinding;
using ReportService.Interfaces;

namespace ReportService.Implementations
{
    public class ApiTask
    {
        public int Id { get; set; }
        public string Schedule { get; set; }
        public string ConnectionString { get; set; }
        public string ViewTemplate { get; set; }
        public string Query { get; set; }
        public string SendAddresses { get; set; }
        public int TryCount { get; set; }
        public int QueryTimeOut { get; set; }
        public int TaskType { get; set; }
    }

    public class ReportsModule : NancyModule
    {
        public ReportsModule(ILogic logic)
        {
            Get["/reports"] = parameters =>
            {
                var response = (Response) $"{logic.GetTaskList_HtmlPage()}";
                response.StatusCode = HttpStatusCode.OK;
                return response;
            };

            Get["/reports/{id:int}"] = parameters =>
            {
                try
                {
                    var response = (Response) $"{logic.GetInstanceList_HtmlPage(parameters.id)}";
                    response.StatusCode = HttpStatusCode.OK;
                    return response;
                }
                catch
                {
                    return HttpStatusCode.NoContent;
                }
            };

            Get["/send"] = parameters =>
            {
                int id = Request.Query.id;
                string mail = Request.Query.address;
                string sentReps = logic.ForceExecute(id, mail);
                var response = sentReps != ""
                    ? (Response) $"Reports {sentReps} sent!"
                    : (Response) "No reports for those ids found...";
                response.StatusCode = sentReps != "" ? HttpStatusCode.OK : HttpStatusCode.NoContent;
                return response;
            };

            Post["/databases"] = parameters => // todo:methods
            {
                try
                {
                    logic.CreateBase(parameters.ConnectionString);
                    return "DataBase successful created!";
                }
                catch (Exception e)
                {
                    return $"DataBase was not created...{e.Message}";
                }
            };

            Delete["/reports/{id:int}"] = parameters =>
            {
                try
                {
                    logic.DeleteTask(parameters.id);
                    return $"deleted task {parameters.id}";
                }
                catch (Exception e)
                {
                    return $"Ошибка: {e.Message}";
                }
            };

            Post["/reports"] = parameters =>
            {
                try
                {
                    var newTask = this.Bind<ApiTask>();
                    var id = logic.CreateTask(newTask);
                    return $"created task {id}";
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    throw;
                }
            };

            Put["/reports"] = parameters =>
            {
                try
                {
                    var existingTask = this.Bind<ApiTask>();
                    logic.UpdateTask(existingTask);
                    return $"updated task {existingTask.Id}";
                }
                catch (Exception e)
                {
                    return $"Ошибка: {e.Message}";
                }
            };

        }
    } //class
}
