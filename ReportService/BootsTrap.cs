using Autofac;
using Nancy;
using Nancy.Bootstrapper;
using Nancy.Bootstrappers.Autofac;
using ReportService.Implementations;
using ReportService.Interfaces;
using ReportService.PluginFolder;

namespace ReportService
{
    public class Bootstrapper : AutofacNancyBootstrapper
    {
        public ILifetimeScope Container => ApplicationContainer;

        protected override void ApplicationStartup(ILifetimeScope container, IPipelines pipelines)
        {
            // No registrations should be performed in here, however you may
            // resolve things that are needed during application startup.
            ILogic log = Container.Resolve<ILogic>();
            log.Start();
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
                        .Named<IDataExecutor>("commondataex")
                        .SingleInstance());

            existingContainer.Update(builder => builder
                        .RegisterType<Daily_DataEx>()
                        .Named<IDataExecutor>("dailyreport_de")
                        .SingleInstance());

            existingContainer.Update(builder => builder
                       .RegisterType<TableViewExecutor>()
                        .Named<IViewExecutor>("tableviewex")
                        .SingleInstance());

            existingContainer.Update(builder => builder
                        .RegisterType<ViewExecutor>()
                        .Named<IViewExecutor>("commonviewex")
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
