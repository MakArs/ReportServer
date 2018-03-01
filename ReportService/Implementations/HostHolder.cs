using System;
using Nancy;
using Nancy.Hosting.Self;
using ReportService.Interfaces;

namespace ReportService.Implementations
{
    public class HostHolder : IHostHolder
    {
        private NancyHost nanHost = new NancyHost(new Uri($"http://localhost:12345/"));

        public HostHolder() { }

        public void Start()
        {
            nanHost.Start();
        }

        public void Stop()
        {
            nanHost.Stop();
        }
    }

    public class ReportStatusModule : NancyModule
    {
        public ILogic logic_;
        public ReportStatusModule(IViewExecutor someView, IDataExecutor someData, IConfig conf, ILogic logic)
        {
            logic_ = logic;

            Get["/reports"] = parameters =>
            {
                return $"{someView.Execute("boo", someData.Execute("select * from instance", 5))}";
            };

            Get["/send"] = parameters =>
            {
                int id = Request.Query.id;
                string mail = Request.Query.address;

                string sentReps = logic.ForceExecute(id, mail);
                return sentReps != "" ? $"Reports {sentReps} sent!" : "No reports for those ids found...";
            };
        }
    }
}
