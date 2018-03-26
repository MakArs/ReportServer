using System.Configuration;
using Autofac;
using Monik.Client;
using Nancy;
using Nancy.Bootstrapper;
using Nancy.Bootstrappers.Autofac;
using ReportService.Implementations;
using ReportService.Interfaces;

public interface IPrivateBootstrapper
{
    void PrivateConfigureApplicationContainer(ILifetimeScope existingContainer);
}

namespace ReportService
{
    public partial class Bootstrapper : AutofacNancyBootstrapper
    {
        public static ILifetimeScope Global;

        public ILifetimeScope Container => ApplicationContainer;

        protected override void ApplicationStartup(ILifetimeScope container, IPipelines pipelines)
        {
            Global = Container;
            // No registrations should be performed in here, however you may
            // resolve things that are needed during application startup.
            ILogic log = Container.Resolve<ILogic>();
            log.Start();
        }

        protected override void ConfigureApplicationContainer(ILifetimeScope existingContainer)
        {
            // Perform registration that should have an application lifetime
            existingContainer.RegisterSingleton<IRepository, Repository>();
            existingContainer.RegisterNamedImplementation<IDataExecutor, DataExecutor>("commondataex");
            existingContainer.RegisterNamedImplementation<IViewExecutor, ViewExecutor>("commonviewex");
            existingContainer.RegisterSingleton<IPostMaster, PostMasterWork>();
            existingContainer.RegisterSingleton<ILogic, Logic>();
            existingContainer.RegisterImplementation<IRTask, RTask>();

            // Configure Monik

            var logSender = new AzureSender(
                ConfigurationManager.AppSettings["monikendpoint"]
                , "incoming");

            existingContainer.RegisterInstance<IClientSender, AzureSender>(logSender);

            var monikSettings = new ClientSettings()
            {
                SourceName = "ReportServer",
                InstanceName = ConfigurationManager.AppSettings["InstanceName"],
                AutoKeepAliveEnable = true
            };
            existingContainer.RegisterInstance<IClientSettings, ClientSettings>(monikSettings);

            existingContainer.RegisterSingleton<IClientControl, MonikInstance>();

            existingContainer.Update(builder => builder
                .Register(c => existingContainer));

            // Partial bootstrapper
            IPrivateBootstrapper privboots = this as IPrivateBootstrapper;
            if (privboots != null)
                privboots.PrivateConfigureApplicationContainer(existingContainer);
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

    public static class LifeTimeExtension
    {
        public static void RegisterSingleton<TInterface, TImplementation>(this ILifetimeScope container)
        {
            container.Update(builder => builder
                .RegisterType<TImplementation>()
                .As<TInterface>()
                .SingleInstance());
        }

        public static void RegisterImplementation<TInterface, TImplementation>(this ILifetimeScope container)
        {
            container.Update(builder => builder
                .RegisterType<TImplementation>()
                .As<TInterface>());
        }

        public static void RegisterInstance<TInterface, TImplementation>(this ILifetimeScope container, TImplementation aInstance)
        {
            container.Update(builder => builder
                .Register(x => aInstance)
                .As<TInterface>());
        }

        public static void RegisterNamedSingleton<TInterface, TImplementation>(this ILifetimeScope container,
            string name)
        {
            container.Update(builder => builder
                .RegisterType<TImplementation>()
                .Named<TInterface>(name)
                .SingleInstance());
        }

        public static void RegisterNamedImplementation<TInterface, TImplementation>(this ILifetimeScope container,
            string name)
        {
            container.Update(builder => builder
                .RegisterType<TImplementation>()
                .Named<TInterface>(name));
        }
    }
}
