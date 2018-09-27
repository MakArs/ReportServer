using System;
using System.Configuration;
using System.Net;
using Autofac;
using AutoMapper;
using Monik.Client;
using Monik.Common;
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
            RegisterNamedDataImporter<DbImporter,DbImporterConfig>
                (existingContainer, "CommonDbImporter");

            RegisterNamedDataImporter<ExcelImporter,ExcelImporterConfig>
                (existingContainer, "CommonExcelImporter");

            RegisterNamedDataExporter<EmailDataSender, EmailExporterConfig>
                (existingContainer, "CommonEmailSender");

            RegisterNamedDataExporter<TelegramDataSender, TelegramExporterConfig>
                (existingContainer, "CommonTelegramSender");

            RegisterNamedDataExporter<DbExporter, DbExporterConfig>
                (existingContainer, "CommonDbExporter");

            RegisterNamedDataExporter<ReportInstanceExporter, ReportInstanceExporterConfig>
                (existingContainer, "CommonReportInstanceExporter");

            RegisterNamedViewExecutor<CommonViewExecutor>(existingContainer, "commonviewex");
            RegisterNamedViewExecutor<CommonTableViewExecutor>(existingContainer, "CommonTableViewEx");
       
            existingContainer
                .RegisterSingleton<ILogic, Logic>();
            existingContainer
                .RegisterImplementation<IRTask, RTask>();


            // Partial bootstrapper for private named implementations registration
            (this as IPrivateBootstrapper)?
                .PrivateConfigureApplicationContainer(existingContainer);

            #region ConfigureMonik

            var logSender = new AzureSender(
                ConfigurationManager.AppSettings["monikendpoint"],
                "incoming");

            existingContainer
                .RegisterInstance<IMonikSender, AzureSender>(logSender);

            var monikSettings = new ClientSettings()
            {
                SourceName = "ReportServer",
                InstanceName = ConfigurationManager.AppSettings["InstanceName"],
                AutoKeepAliveEnable = true
            };

            existingContainer
                .RegisterInstance<IMonikSettings, ClientSettings>(monikSettings);

            existingContainer
                .RegisterSingleton<IMonik, MonikClient>();

            #endregion

            var repository = new Repository(ConfigurationManager.AppSettings["DBConnStr"],
                existingContainer.Resolve<IMonik>());

            existingContainer
                .RegisterInstance<IRepository, Repository>(repository);

            #region ConfigureMapper

            var mapperConfig =
                new MapperConfiguration(cfg => cfg.AddProfile(typeof(MapperProfile)));
            // Hint: add to ctor if many profileSs needed: cfg.AddProfile(typeof(AutoMapperProfile));
            existingContainer.RegisterSingleInstance<MapperConfiguration, MapperConfiguration>(
                mapperConfig);
            var mapper = existingContainer.Resolve<MapperConfiguration>().CreateMapper();
            existingContainer.RegisterSingleInstance<IMapper, IMapper>(mapper);

            #endregion

            existingContainer.Update(builder=>builder
                .RegisterType<DefaultTaskWorker>());

            existingContainer.RegisterImplementation<IArchiver,Archiver7Zip>();

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

        private void RegisterNamedDataExporter<TImplementation, TConfigType>
            (ILifetimeScope container, string name)
            where TImplementation : IDataExporter
            where TConfigType : IExporterConfig
        {
            container.Update(builder => builder
                .RegisterType<TImplementation>()
                .Named<IDataExporter>(name)
                .Keyed<IDataExporter>(typeof(TConfigType)));
        }

        private void RegisterNamedDataImporter<TImplementation, TConfigType>
            (ILifetimeScope container, string name) 
            where TImplementation : IDataImporter
            where TConfigType : IImporterConfig
        {
            container.Update(builder => builder
                .RegisterType<TImplementation>()
                .Named<IDataImporter>(name)
                .Keyed<IDataImporter>(typeof(TConfigType)));
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

            CreateMap<DtoOperInstance, ApiOperInstance>()
                .ForMember("DataSet", opt => opt.Ignore());

            CreateMap<DtoOperInstance, DtoTaskInstance>();

            CreateMap<DbExporterConfig, DbExporter>();
            CreateMap<EmailExporterConfig, EmailDataSender>();
            CreateMap<TelegramExporterConfig, TelegramDataSender>();
            CreateMap<ReportInstanceExporterConfig, ReportInstanceExporter>();
            CreateMap<DbImporterConfig, DbImporter>();
            CreateMap<ExcelImporterConfig, ExcelImporter>();
        }
    }
}
