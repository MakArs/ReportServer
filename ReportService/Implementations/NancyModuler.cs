using System;
using Nancy;
using ReportService.Interfaces;

namespace ReportService.Implementations
{
    public class ReportStatusModule : NancyModule
    {
        public ReportStatusModule(ILogic logic)
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

            Get["/create/{id:int}"] = parameters =>
            {
                try
                {
                    logic.CreateTask(((Logic)logic)._tasks[parameters.id]);
                    return $"created copy of task {parameters.id}";
                }
                catch(Exception e)
                {
                    return $"Ошибка: {e.Message}";
                }
            };

            Get["/update/{id:int}"] = parameters =>
            {
                try
                {
                    logic.UpdateTask( ((Logic)logic)._tasks[0]);
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
