using System.Runtime.CompilerServices;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using AutoMapper;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Monik.Client;
using Monik.Common;
using ReportService.Core;
using ReportService.Entities;
using ReportService.Extensions;
using ReportService.Interfaces.Core;

namespace ReportService
{
    public class Program
    {
        public static ILifetimeScope Container;
        public static void Main(string[] args)
        {
            
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .UseServiceProviderFactory(new AutofacServiceProviderFactory())
                .ConfigureContainer<ContainerBuilder>(ConfigureContainer)
                .ConfigureServices((hostContext, services) =>
                {
                    services.AddHostedService<Worker>();
                });

        private static void ConfigureContainer(ContainerBuilder builder)
        {
            var configBuilder = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json");
            var config = configBuilder.Build();
            
            //existingContainer.RegisterSingleInstance<ServiceConfiguration, ServiceConfiguration>
            //    (serviceConfiguration);

            //RegisterNamedDataImporter<DbImporter, DbImporterConfig>
            //    (existingContainer, "CommonDbImporter");

            //RegisterNamedDataImporter<ExcelImporter, ExcelImporterConfig>
            //    (existingContainer, "CommonExcelImporter");

            //RegisterNamedDataImporter<CsvImporter, CsvImporterConfig>
            //    (existingContainer, "CommonCsvImporter");

            //RegisterNamedDataImporter<SshImporter, SshImporterConfig>
            //    (existingContainer, "CommonSshImporter");

            //RegisterNamedDataExporter<EmailDataSender, EmailExporterConfig>
            //    (existingContainer, "CommonEmailSender");

            //RegisterNamedDataExporter<TelegramDataSender, TelegramExporterConfig>
            //    (existingContainer, "CommonTelegramSender");

            //RegisterNamedDataExporter<DbExporter, DbExporterConfig>
            //    (existingContainer, "CommonDbExporter");

            //RegisterNamedDataExporter<B2BExporter, B2BExporterConfig>
            //    (existingContainer, "CommonB2BExporter");

            //RegisterNamedDataExporter<SshExporter, SshExporterConfig>
            //    (existingContainer, "CommonSshExporter");

            //RegisterNamedDataExporter<FtpExporter, FtpExporterConfig>
            //    (existingContainer, "CommonFtpExporter");

            //RegisterNamedViewExecutor<CommonViewExecutor>
            //    (existingContainer, "commonviewex");

            //RegisterNamedViewExecutor<GroupedViewExecutor>
            //    (existingContainer, "GroupedViewex");

            //RegisterNamedViewExecutor<CommonTableViewExecutor>
            //    (existingContainer, "CommonTableViewEx");

            //builder
            //    .RegisterImplementationSingleton<ILogic, Logic>();
            //builder
            //    .RegisterImplementation<IReportTask, ReportTask.ReportTask>();

            //// Partial bootstrapper for private named implementations registration
            //(this as IPrivateBootstrapper)?
            //    .PrivateConfigureApplicationContainer(existingContainer);

            #region ConfigureMonik

            var logSender = new RabbitMqSender(
                config["MonikSettings:EndPoint"],
                "MonikQueue");

            builder
                .RegisterInstance<IMonikSender, RabbitMqSender>(logSender);

            var monikSettings = new ClientSettings
            {
                SourceName = "ReportServer",
                InstanceName = config["MonikSettings:InstanceName"],
                AutoKeepAliveEnable = true
            };

            builder
                .RegisterInstance<IMonikSettings, ClientSettings>(monikSettings);

            builder
                .RegisterImplementationSingleton<IMonik, MonikClient>();

            #endregion

            builder
                .Register(c=>new Repository(config["DBConnStr"],c.Resolve<IMonik>()))
                .As<IRepository>()
                .SingleInstance();

            #region ConfigureMapper

            var mapperConfig =
                new MapperConfiguration(cfg => cfg.AddProfile(typeof(MapperProfile)));

            builder.RegisterSingleInstance<MapperConfiguration, MapperConfiguration>(
                mapperConfig);
            builder.Register(c => c.Resolve<MapperConfiguration>().CreateMapper())
                .As<IMapper>()
                .SingleInstance();

            #endregion

            var rnd = new ThreadSafeRandom();
            builder.RegisterSingleInstance<ThreadSafeRandom, ThreadSafeRandom>(rnd);

            //existingContainer.RegisterImplementation<IDefaultTaskExporter, DefaultTaskExporter>();


            //existingContainer.RegisterNamedImplementation<IArchiver, ArchiverZip>("Zip");
            //existingContainer.RegisterNamedImplementation<IArchiver, Archiver7Zip>("7Zip");

            //switch (serviceConfiguration.ArchiveFormat)
            //{
            //    case "Zip":
            //        existingContainer.RegisterImplementation<IArchiver, ArchiverZip>();
            //        break;
            //    case "7Zip":
            //        existingContainer.RegisterImplementation<IArchiver, Archiver7Zip>();
            //        break;
            //}


            //existingContainer.RegisterImplementation<IReportTaskRunContext, ReportTaskRunContext>();

            //existingContainer.RegisterImplementation<ITaskWorker, TaskWorker>();

            //existingContainer.RegisterImplementation<IPackageBuilder, ProtoPackageBuilder>();
            //existingContainer.RegisterImplementation<IPackageParser, ProtoPackageParser>();

            //existingContainer.RegisterImplementation<IProtoSerializer, ProtoSerializer>();

            #region ConfigureBot

            //Uri proxyUri = new Uri(serviceConfiguration.ProxySettings.ProxyUriAddr);
            //ICredentials credentials = new NetworkCredential(
            //    serviceConfiguration.ProxySettings.ProxyLogin,
            //    serviceConfiguration.ProxySettings.ProxyPassword);
            //WebProxy proxy = new WebProxy(proxyUri, true, null, credentials);
            //TelegramBotClient bot =
            //    new TelegramBotClient(serviceConfiguration.BotToken, proxy);
            //existingContainer
            //    .RegisterSingleInstance<ITelegramBotClient, TelegramBotClient>(bot);

            #endregion

            //existingContainer //why?
            //    .RegisterInstance<ILifetimeScope, ILifetimeScope>(existingContainer);

            //existingContainer.RegisterInstance<TokenValidationSettings, TokenValidationSettings>(
            //    new TokenValidationSettings
            //    {
            //        Audience = serviceConfiguration.TokenValidationSettings.Token_Audience,
            //        Issuer = serviceConfiguration.TokenValidationSettings.Token_Issuer,
            //        Keys = new[]
            //        {
            //            new KeyInfo
            //            {
            //                Key = serviceConfiguration.TokenValidationSettings.Token_Secret,
            //                Alg = serviceConfiguration.TokenValidationSettings.Token_Alg
            //            }
            //        }
            //    });
        }

    }
}