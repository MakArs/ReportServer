using System;
using System.Threading.Tasks;
using Autofac;
using Autofac.Features.Indexed;
using Monik.Client;
using Nancy;
using Nancy.Hosting.Self;
using ReportService.Interfaces;

namespace ReportService.Implementations
{
    public class HostHolder : IHostHolder
    {
        private readonly IClientControl _monik;
        private readonly NancyHost _nancyHost;

        public HostHolder()
        {
            _nancyHost = new NancyHost(
                new Uri("http://localhost:12345"), 
                new Bootstrapper(), 
                HostConfigs);

            _monik = Bootstrapper.Global.Resolve<IClientControl>();
            _monik.ApplicationInfo("HostHolder.ctor");
        }

        public static HostConfiguration HostConfigs = new HostConfiguration()
        {
            UrlReservations = new UrlReservations() {CreateAutomatically = true}, RewriteLocalhost = true
        };

        public void Start()
        {
            _monik.ApplicationWarning("Started");

            try
            {
                _nancyHost.Start();
            }
            catch (Exception e)
            {
                _monik.ApplicationError(e.Message);
            }
        }

        public void Stop()
        {
            try
            {
                _nancyHost.Stop();
            }
            catch (Exception e)
            {
                _monik.ApplicationError(e.Message);
            }

            _monik.ApplicationWarning("Stopped");
            _monik.OnStop();
        }
    }

    public class ReportStatusModule : NancyModule
    {
        private readonly ILogic _logic;

        public ReportStatusModule(ILogic logic)
        {
            _logic = logic;

            Get["/report"] = parameters => { return $"{_logic.GetTaskView()}"; };

            Get["/send"] = parameters =>
            {
                int id = Request.Query.id;
                string mail = Request.Query.address;

                string sentReps = _logic.ForceExecute(id, mail);
                return sentReps != "" ? $"Reports {sentReps} sent!" : "No reports for those ids found...";
            };

            Get["/report/{id:int}"] = parameters =>
            {
                try
                {
                    return $"{_logic.GetInstancesView(parameters.id)}";
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
                    logic.CreateTask(((Logic)logic)._tasks[parameters.id]);
                    return $"created copy of task {parameters.id}";
                }
                catch
                {
                    return "no task with this id found...";
                }
            };

            Get["/update/{id:int}"] = parameters =>
            {
                try
                {
                    logic.UpdateTask(parameters.id,((Logic)logic)._tasks[0]);
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
