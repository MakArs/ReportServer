using System.Configuration;
using Autofac;
using AutoMapper;
using Monik.Client;
using Nancy;
using Nancy.Bootstrapper;
using Nancy.Bootstrappers.Autofac;
using ReportService.Core;
using ReportService.Interfaces;
using ReportService.Nancy;
using ReportService.View;

namespace ReportService
{
    public interface IPrivateBootstrapper
    {
        void PrivateConfigureApplicationContainer(ILifetimeScope existingContainer);
    }

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
            existingContainer.RegisterNamedImplementation<IDataExecutor, DataExecutor>("commondataex");
            existingContainer.RegisterNamedImplementation<IViewExecutor, ViewExecutor>("commonviewex");
            existingContainer.RegisterNamedImplementation<IViewExecutor, TaskListViewExecutor>("tasklistviewex");
            existingContainer
                .RegisterNamedImplementation<IViewExecutor, InstanceListViewExecutor>("instancelistviewex");
            existingContainer.RegisterSingleton<IPostMaster, PostMasterWork>();
            existingContainer.RegisterSingleton<ILogic, Logic>();
            existingContainer.RegisterImplementation<IRTask, RTask>();

            var repository = new Repository(ConfigurationManager.AppSettings["DBConnStr"]);
            existingContainer.RegisterInstance<IRepository, Repository>(repository);

            // Configure Monik
            var logSender = new AzureSender(
                ConfigurationManager.AppSettings["monikendpoint"],
                "incoming");

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

            //mapper instance
            var mapperConfig = new MapperConfiguration(cfg => cfg.AddProfile(typeof(MapperProfile)));
            // Hint: add to ctor if many profiles needed: cfg.AddProfile(typeof(AutoMapperProfile));

            existingContainer.RegisterSingleInstance<MapperConfiguration, MapperConfiguration>(mapperConfig);
            var mapper = existingContainer.Resolve<MapperConfiguration>().CreateMapper();
            existingContainer.RegisterInstance<IMapper, IMapper>(mapper);
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

        public static void RegisterInstance<TInterface, TImplementation>(this ILifetimeScope container,
            TImplementation aInstance)
        {
            container.Update(builder => builder
                .Register(x => aInstance)
                .As<TInterface>());
        }

        public static void RegisterSingleInstance<TInterface, TImplementation>(this ILifetimeScope container,
            TImplementation aInstance)
        {
            container.Update(builder => builder
                .Register(x => aInstance)
                .As<TInterface>()
                .SingleInstance());
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
    } //extensions

    public class MapperProfile : Profile
    {
        public MapperProfile()
        {
            CreateMap<DtoSchedule, RSchedule>();
            CreateMap<DtoRecepientGroup, RRecepientGroup>();

            CreateMap<ApiFullTask, DtoTask>();
             // .ForMember("ConnectionString", opt => opt.MapFrom(s => s.ConnectionString == "" ? null : s.ConnectionString));
            CreateMap<RTask, ApiFullTask>()
                .ForMember("ScheduleId", opt => opt.MapFrom(s => s.Schedule.Id))
                .ForMember("RecepientGroupId", opt => opt.MapFrom(s => s.SendAddresses.Id))
                .ForMember("ReportType", opt => opt.MapFrom(s => (int)s.Type));
            CreateMap<RTask, ApiTask>()
                .ForMember("RecepientGroupId", opt => opt.MapFrom(s => s.SendAddresses.Id))
                .ForMember("ScheduleId", opt => opt.MapFrom(s => s.Schedule.Id));

            CreateMap<DtoReport, RReport>();
            CreateMap<DtoFullInstance, DtoInstance>();
            CreateMap<DtoFullInstance, DtoInstanceData>()
                .ForMember("InstanceId", opt => opt.MapFrom(s => s.Id));
            CreateMap<DtoFullInstance, ApiInstance>();
            CreateMap<DtoInstance, ApiInstanceCompact>();
        }
    }
}
