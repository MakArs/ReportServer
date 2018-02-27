using System;
using Nancy;
using Nancy.Hosting.Self;
using ReportService.Interfaces;
using Nancy.Bootstrappers.Autofac;
using Autofac;

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

    public class Bootstrapper : AutofacNancyBootstrapper
    {
        protected override void ConfigureApplicationContainer(ILifetimeScope container)
        {
            container.Update(builder => builder.RegisterType<Logic>().As<ILogic>().SingleInstance());
        }
    }


    public class ReportStatusModule : NancyModule
    {
        public ReportStatusModule(IViewExecutor someView, IDataExecutor someData, IConfig conf,ILogic logic)
        {
            Get["/reports"] = parameters =>
            {
                return $"{someView.Execute(1, someData.Execute("select * from instance"))}";
            };
            Put["/send/{id:int}"] = parameters =>
            {
                return $"Reports {logic.ForceExecute(parameters.id)} was sended!";
            };
        }
    }
}
