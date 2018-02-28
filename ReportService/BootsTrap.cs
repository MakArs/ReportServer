using Autofac;
using Nancy;
using Nancy.Bootstrapper;
using Nancy.Bootstrappers.Autofac;
using ReportService.Implementations;
using ReportService.Interfaces;
using System.Threading.Tasks;

namespace ReportService
{
    public class Bootstrapper : AutofacNancyBootstrapper
    {
        protected override void ApplicationStartup(ILifetimeScope container, IPipelines pipelines)
        {
            // No registrations should be performed in here, however you may
            // resolve things that are needed during application startup.
            ILogic log = container.Resolve<ILogic>();
            Task t = new Task(()=>log.Execute());
            t.Start();
            //log.Start();
        }

        protected override void ConfigureApplicationContainer(ILifetimeScope existingContainer)
        {
            // Perform registration that should have an application lifetime
            existingContainer.Update(builder => builder
                        .RegisterType<ConfigTest>()
                        .As<IConfig>()
                        .SingleInstance());

            existingContainer.Update(builder => builder
                        .RegisterType<DataExecutorTest>()
                        .As<IDataExecutor>()
                        .SingleInstance());

            existingContainer.Update(builder => builder
                        .RegisterType<ViewExecutor>()
                        .As<IViewExecutor>()
                        .SingleInstance());

            existingContainer.Update(builder => builder
                        .RegisterType<PostMasterWork>()
                        .As<IPostMaster>()
                        .SingleInstance());

            existingContainer.Update(builder => builder
                        .RegisterType<Logic>()
                        .As<ILogic>()
                        .SingleInstance());

            existingContainer.Update(builder => builder
                                    .RegisterType<RTask>()
                                    .As<IRTask>());

            existingContainer.Update(builder => builder
                                   .Register(c => existingContainer));
        }

        protected override void ConfigureRequestContainer(ILifetimeScope container, NancyContext context)
        {
            // Perform registrations that should have a request lifetime
        }

        protected override void RequestStartup(ILifetimeScope container, IPipelines pipelines, NancyContext context)
        {
            // No registrations should be performed in here, however you may
            // resolve things that are needed during request startup.
        }
    }
}
