using System;
using Nancy;
using Nancy.Hosting.Self;
using ReportService.Interfaces;

namespace ReportService.Implementations
{
    public class HostHolder : IHostHolder
    {
        private NancyHost nanHost = new NancyHost(new Uri($"http://localhost:12345/"));
        public void Start()
        {
            nanHost.Start();
        }

        public void Stop()
        {
            nanHost.Stop();
        }
    }
    public class ReportStatusModule : NancyModule //TODO: add query or html page to constructor
    {
        public ReportStatusModule()
        {
            Get["/reports"] = parameters =>
            {
                return $"some view  logic";
            };
        }
    }
}
