using System;
using Nancy;
using ReportService.Interfaces;

namespace ReportService.Implementations
{
    public class ReportStatusModule : NancyModule
    {
        public ReportStatusModule(ILogic logic)
        {
            Get["/report"] = parameters => $"{logic.GetTaskView()}";

            Get["/send"] = parameters =>
            {
                int id = Request.Query.id;
                string mail = Request.Query.address;

                string sentReps = logic.ForceExecute(id, mail);
                return sentReps != "" ? $"Reports {sentReps} sent!" : "No reports for those ids found...";
            };

            Get["/report/{id:int}"] = parameters =>
            {
                try
                {
                    return $"{logic.GetInstancesView(parameters.id)}";
                }
                catch
                {
                    return "no report with this id found...";
                }
            };

            Put["/createdatabase/{ConnectionString}"] = parameters =>
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

            Get["/delete/{id:int}"] = parameters =>
            {
                try
                {
                    logic.DeleteTask(parameters.id);
                    return $"deleted task {parameters.id}";
                }
                catch
                {
                    return "no task with this id found...";
                }
            };

            Get["/create/{id:int}"] = parameters =>
            {
                try
                {
                   // var tsk = ((Logic) logic)._tasks[parameters.id];
                    //logic.CreateTask(tsk);
                    return $"created copy of task {parameters.id}";
                }
                catch(Exception e)
                {
                    return "no task with this id found...";
                }
            };

            Get["/update/{id:int}"] = parameters =>
            {
                try
                {
                    //logic.UpdateTask(parameters.id, ((Logic)logic)._tasks[0]);
                    return $"updated task {parameters.id}";
                }
                catch
                {
                    return "no task with this id found...";
                }
            };

        }
    } //class
}
