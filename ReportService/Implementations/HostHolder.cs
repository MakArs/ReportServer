﻿using System;
using Autofac.Features.Indexed;
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
        public ReportStatusModule(ILogic logic)
        {
            logic_ = logic;
            Get["/report"] = parameters =>
            {
                return $"{logic_.GetTaskView()}";
            };

            Get["/send"] = parameters =>
            {
                int id = Request.Query.id;
                string mail = Request.Query.address;

                string sentReps = logic_.ForceExecute(id, mail);
                return sentReps != "" ? $"Reports {sentReps} sent!" : "No reports for those ids found...";
            };

            Get["/report/{id:int}"] = parameters =>
            {
                logic.CreateBase(@"Data Source=WS-00005; Initial Catalog=ReportBase; Integrated Security=True");
                return $"{logic_.GetInstancesView(parameters.id)}";
            };
        }
    }
}