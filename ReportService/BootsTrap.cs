using System;
using System.Configuration;
using System.IO;
using System.Net;
using System.Reflection;
using Autofac;
using AutoMapper;
using Monik.Client;
using Nancy;
using Nancy.Bootstrapper;
using Nancy.Bootstrappers.Autofac;
using ReportService.Core;
using ReportService.DataExporters;
using ReportService.DataExporters.ViewExecutors;
using ReportService.DataImporters;
using ReportService.Extensions;
using ReportService.Interfaces;
using ReportService.Nancy;
using SevenZip;
using Telegram.Bot;

namespace ReportService
{
    public interface IPrivateBootstrapper
    {
        void PrivateConfigureApplicationContainer(ILifetimeScope existingContainer);
    }

    public partial class Bootstrapper : AutofacNancyBootstrapper
    {
        public static ILifetimeScope Global; //mech of work?

        public ILifetimeScope Container => ApplicationContainer;

        protected override void ApplicationStartup(ILifetimeScope container, IPipelines pipelines)
        {
            Global = Container;
            ILogic log = Container.Resolve<ILogic>();
            log.Start();
        }

        protected override void ConfigureApplicationContainer(ILifetimeScope existingContainer)
        {
            RegisterNamedDataImporter<DbImporter>(existingContainer, "CommonDbImporter");
            RegisterNamedDataImporter<ExcelImporter>(existingContainer, "CommonExcelImporter");

            RegisterNamedDataExporter<EmailDataSender>(existingContainer,"CommonEmailSender");
            RegisterNamedDataExporter<TelegramDataSender>(existingContainer, "CommonTelegramSender");
            RegisterNamedDataExporter<DbExporter>(existingContainer, "CommonDbExporter");

            RegisterNamedViewExecutor<CommonViewExecutor>(existingContainer, "commonviewex");
            RegisterNamedViewExecutor<TaskListViewExecutor>(existingContainer, "tasklistviewex");
            RegisterNamedViewExecutor<InstanceListViewExecutor>(existingContainer,
                "instancelistviewex");

            existingContainer
                .RegisterSingleton<ILogic, Logic>();
            existingContainer
                .RegisterImplementation<IRTask, RTask>();

            var repository = new Repository(ConfigurationManager.AppSettings["DBConnStr"]);
            existingContainer
                .RegisterInstance<IRepository, Repository>(repository);

            // Partial bootstrapper for private named implementations registration
            IPrivateBootstrapper privboots = this as IPrivateBootstrapper;
            if (privboots != null)
                privboots
                    .PrivateConfigureApplicationContainer(existingContainer);

            #region ConfigureMonik

            var logSender = new AzureSender(
                ConfigurationManager.AppSettings["monikendpoint"],
                "incoming");

            existingContainer
                .RegisterInstance<IClientSender, AzureSender>(logSender);

            var monikSettings = new ClientSettings()
            {
                SourceName = "ReportServer",
                InstanceName = ConfigurationManager.AppSettings["InstanceName"],
                AutoKeepAliveEnable = true
            };

            existingContainer
                .RegisterInstance<IClientSettings, ClientSettings>(monikSettings);

            existingContainer
                .RegisterSingleton<IClientControl, MonikInstance>();

            #endregion

            #region ConfigureMapper

            var mapperConfig =
                new MapperConfiguration(cfg => cfg.AddProfile(typeof(MapperProfile)));
            // Hint: add to ctor if many profileSs needed: cfg.AddProfile(typeof(AutoMapperProfile));
            existingContainer.RegisterSingleInstance<MapperConfiguration, MapperConfiguration>(
                mapperConfig);
            var mapper = existingContainer.Resolve<MapperConfiguration>().CreateMapper();
            existingContainer.RegisterSingleInstance<IMapper, IMapper>(mapper);

            #endregion

            #region ConfigureCompressor

            var path = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location),
                Environment.Is64BitProcess ? "x64" : "x86", "7z.dll");
            SevenZipBase.SetLibraryPath(path);
            var compressor = new SevenZipCompressor
            {
                CompressionMode = CompressionMode.Create,
                ArchiveFormat = OutArchiveFormat.SevenZip
            };
            var archiver = new Archiver7Zip(compressor);
            existingContainer.RegisterSingleInstance<IArchiver, Archiver7Zip>(archiver);

            #endregion

            #region ConfigureBot

            Uri proxyUri = new Uri(ConfigurationManager.AppSettings["proxyUriAddr"]);
            ICredentials credentials = new NetworkCredential(
                ConfigurationManager.AppSettings["proxyLogin"],
                ConfigurationManager.AppSettings["proxyPassword"]);
            WebProxy proxy = new WebProxy(proxyUri, true, null, credentials);
            TelegramBotClient bot =
                new TelegramBotClient(ConfigurationManager.AppSettings["BotToken"], proxy);
            existingContainer
                .RegisterSingleInstance<ITelegramBotClient, TelegramBotClient>(bot);

            #endregion

            existingContainer //why?
                .RegisterInstance<ILifetimeScope, ILifetimeScope>(existingContainer);
        }

        protected override void ConfigureRequestContainer(ILifetimeScope container,
                                                          NancyContext context)
        {
            // Perform registrations that should have a request lifetime
        }

        protected override void RequestStartup(ILifetimeScope container, IPipelines pipelines,
                                               NancyContext context)
        {
            // No registrations should be performed in here, however you may
            // resolve things that are needed during request startup.
        }

        private void RegisterNamedViewExecutor<TImplementation>
            (ILifetimeScope container, string name) where TImplementation : IViewExecutor
        {
            container.Update(builder => builder
                .RegisterType<TImplementation>()
                .Named<IViewExecutor>(name));
        }

        private void RegisterNamedDataExporter<TImplementation>
            (ILifetimeScope container, string name) where TImplementation : IDataExporter
        {
            container.Update(builder => builder
                .RegisterType<TImplementation>()
                .Named<IDataExporter>(name));
        }

        private void RegisterNamedDataImporter<TImplementation>
            (ILifetimeScope container, string name) where TImplementation : IDataImporter
        {
            container.Update(builder => builder
                .RegisterType<TImplementation>()
                    .Named<IDataImporter>(name));
        }
    }

    public class MapperProfile : Profile
    {
        public MapperProfile()
        {
            CreateMap<DtoRecepientGroup, RRecepientGroup>();

            CreateMap<RTask, DtoTask>()
                .ForMember("ScheduleId", opt => opt.MapFrom(s => s.Schedule.Id));

            CreateMap<ApiTask, DtoTask>();

            CreateMap<DtoOperInstance, DtoTaskInstance>();
        }
    }
}
