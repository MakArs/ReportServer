using System;
using AutoMapper;
using Nancy;
using Nancy.Extensions;
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
            Get["/reports"] = parameters => $"{logic.GetTaskList_HtmlPage()}";

            Get["/send"] = parameters =>
            {
                int id = Request.Query.id;
                string mail = Request.Query.address;

                string sentReps = logic.ForceExecute(id, mail);
                return sentReps != "" ? $"Reports {sentReps} sent!" : "No reports for those ids found...";
            };

            Get["/reports/{id:int}"] = parameters =>
            {
                try
                {
                    return $"{logic.GetInstanceList_HtmlPage(parameters.id)}";
                }
                catch
                {
                    return "no report with this id found...";
                }
            };

            Post["/createdatabase/{ConnectionString}"] = parameters => // todo:methods
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

            Delete["/delete/{id:int}"] = parameters =>
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

            Post["/create"] = parameters =>
            {
                //var text = Context.Request.Body.AsString();
                //Mapper.Initialize(conf => conf.CreateMap<Request, ApiTask > ());
                //ApiTask newTask = new ApiTask();
                try
                {
                    var newTask = this.Bind<ApiTask>();
                try
                {
                    var id=logic.CreateTask(newTask);
                    return $"created task {id}";
                }
                catch (Exception e)
                {
                    return $"Ошибка: {e.Message}";
                }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    throw;
                }
            };

            Put["/update"] = parameters =>
            {
                try
                {
                    logic.UpdateTask(new ApiTask());
                    return $"updated task {parameters.id}";
                }
                catch (Exception e)
                {
                    return $"Ошибка: {e.Message}";
                }
            };

        }
    } //class
}
