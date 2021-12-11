using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Autofac;
using AutoMapper;
using ExternalConfiguration;
using Microsoft.Extensions.Configuration;
using Monik.Client;
using Monik.Common;
using ReportService.Core;
using ReportService.Entities;
using ReportService.Extensions;
using ReportService.Interfaces.Core;
using ReportService.Interfaces.Protobuf;
using ReportService.Interfaces.ReportTask;
using ReportService.Operations.DataExporters;
using ReportService.Operations.DataExporters.Configurations;
using ReportService.Operations.DataExporters.Dependencies;
using ReportService.Operations.DataExporters.ViewExecutors;
using ReportService.Operations.DataImporters;
using ReportService.Operations.DataImporters.Configurations;
using ReportService.Protobuf;
using ReportService.ReportTask;
using Telegram.Bot;

namespace ReportService
{
    public interface IPrivateBootstrapper
    {
        void PrivateConfigureApplicationContainer(ContainerBuilder builder);
    }

    public partial class Bootstrapper
    {
        private IConfigurationRoot GetConfiguration()
        {
            var configBuilder = new ConfigurationBuilder().AddJsonFile("ConsulSettings.json");
            IConfigurationRoot config = configBuilder.Build();

            var store = new ConsulConfigurationStore(config["Consul:Url"], config["Consul:Token"]);
            IExternalConfigurationProvider provider = new ExternalConfigurationProvider(store, config["Environment"]);
            Task<Dictionary<string, string>> getConsulSettingsTask = provider.GetServiceSettingsAsync("ReportService");

            configBuilder.Sources.Clear();

            try
            {
                Dictionary<string, string> serviceSettings = getConsulSettingsTask.Result;

                if (!string.IsNullOrEmpty(serviceSettings["AppService"]))
                {
                    var jsonStream = new MemoryStream(Encoding.UTF8.GetBytes(serviceSettings["AppService"]));
                    configBuilder.AddJsonStream(jsonStream);
                }
            }
            catch (AggregateException)
            {
                configBuilder.AddJsonFile("appsettings.json");
            }

            config = configBuilder.Build();
            return config;
        }

        public void ConfigureContainer(ContainerBuilder builder)
        {
            var config = GetConfiguration();
            builder.RegisterSingleInstance<IConfigurationRoot, IConfigurationRoot>(config);
            
            builder.RegisterNamedDataImporter<DbImporter, DbImporterConfig>("CommonDbImporter");
            builder.RegisterNamedDataImporter<PostgresDbImporter, DbImporterConfig>("PostgresDbImporter");
            builder.RegisterNamedDataImporter<ExcelImporter, ExcelImporterConfig>("CommonExcelImporter");
            builder.RegisterNamedDataImporter<CsvImporter, CsvImporterConfig>("CommonCsvImporter");
            builder.RegisterNamedDataImporter<SshImporter, SshImporterConfig>("CommonSshImporter");
            builder.RegisterNamedDataImporter<EmailAttachementImporter, EmailImporterConfig>("CommonEmailImporter");
            builder.RegisterNamedDataExporter<EmailDataSender, EmailExporterConfig>("CommonEmailSender");
            builder.RegisterNamedDataExporter<TelegramDataSender, TelegramExporterConfig>("CommonTelegramSender");
            builder.RegisterNamedDataExporter<DbExporter, DbExporterConfig>("CommonDbExporter");
            builder.RegisterNamedDataExporter<PostgresDbExporter, DbExporterConfig>("PostgresDbExporter");
            builder.RegisterNamedDataExporter<B2BExporter, B2BExporterConfig>("CommonB2BExporter");
            builder.RegisterNamedDataExporter<PostgresB2BExporter, B2BExporterConfig>("PostgresB2BExporter");
            builder.RegisterNamedDataExporter<SshExporter, SshExporterConfig>("CommonSshExporter");
            builder.RegisterNamedDataExporter<FtpExporter, FtpExporterConfig>("CommonFtpExporter");

            builder.RegisterNamedViewExecutor<CommonViewExecutor>("commonviewex");
            builder.RegisterNamedViewExecutor<GroupedViewExecutor>("GroupedViewex");
            builder.RegisterNamedViewExecutor<CommonTableViewExecutor>("CommonTableViewEx");

            builder.RegisterImplementationSingleton<ILogic, Logic>();

            builder.RegisterImplementation<IReportTask, ReportTask.ReportTask>();

            // Partial bootstrapper for private named implementations registration
            (this as IPrivateBootstrapper)?.PrivateConfigureApplicationContainer(builder);

            ConfigureMonik(builder, config);

            builder.RegisterNamedImplementation<IRepository, SqlServerRepository>("SQLServer");
            builder.RegisterNamedImplementation<IRepository, PostgreSqlRepository>("PostgreSQL");
            builder.RegisterImplementation<IDBStructureChecker, B2BDBStructureChecker>();
            //TODO implement cross support for MSql server and PostgreSql
            //builder.RegisterImplementation<IDBStructureChecker, PostgresDBStructureChecker>();

            builder.Register(c =>
            {
                var repos = c.ResolveNamed<IRepository>(config["DBMS"],
                    new NamedParameter("connStr", config["DBConnStr"]),

                    new NamedParameter("monik", c.Resolve<IMonik>()));
                return repos;
            })
                .As<IRepository>()
                .SingleInstance();


            ConfigureMapper(builder);

            var rnd = new ThreadSafeRandom();
            builder.RegisterSingleInstance<ThreadSafeRandom, ThreadSafeRandom>(rnd);

            builder.RegisterImplementation<IDefaultTaskExporter, DefaultTaskExporter>();

            builder.RegisterNamedImplementation<IArchiver, ArchiverZip>("Zip");
            builder.RegisterNamedImplementation<IArchiver, Archiver7Zip>("7Zip");

            switch (config["ArchiveFormat"])
            {
                case "Zip":
                    builder.RegisterImplementation<IArchiver, ArchiverZip>();
                    break;
                case "7Zip":
                    builder.RegisterImplementation<IArchiver, Archiver7Zip>();
                    break;
            }

            builder.RegisterImplementation<IReportTaskRunContext, ReportTaskRunContext>();

            builder.RegisterImplementation<ITaskWorker, TaskWorker>();

            builder.RegisterImplementation<IPackageBuilder, ProtoPackageBuilder>();
            builder.RegisterImplementation<IPackageParser, ProtoPackageParser>();

            builder.RegisterImplementation<IProtoSerializer, ProtoSerializer>();

            builder.RegisterImplementation<IEmailClientService, EmailClientService>();

            ConfigureTelegramBot(builder, config);
        }

        private void ConfigureMapper(ContainerBuilder builder)
        {
            builder.RegisterType<MapperProfile>().As<Profile>().SingleInstance();

            builder.Register(c =>
                {
                    var profiles = c.Resolve<IEnumerable<Profile>>();

                    var mapperConfig =
                        new MapperConfiguration(cfg =>
                        {
                            foreach (var prof in profiles)
                                cfg.AddProfile(prof);
                        });

                    return mapperConfig.CreateMapper();
                })
                .As<IMapper>()
                .SingleInstance();
        }

        private void ConfigureTelegramBot(ContainerBuilder builder, IConfigurationRoot config)
        {
            Uri proxyUri = new Uri(config["ProxySettings:ProxyUriAddr"]);

            ICredentials credentials = new NetworkCredential(
                config["ProxySettings:ProxyLogin"],
                config["ProxySettings:ProxyPassword"]);
            WebProxy proxy = new WebProxy(proxyUri, true, null, credentials);
            TelegramBotClient bot =
                new TelegramBotClient(config["BotToken"], proxy);
            builder
                .RegisterSingleInstance<ITelegramBotClient, TelegramBotClient>(bot);
        }

        private void ConfigureMonik(ContainerBuilder builder, IConfigurationRoot config)
        {
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

            builder.RegisterInstance<IMonikSettings, ClientSettings>(monikSettings);

            builder.RegisterImplementationSingleton<IMonik, MonikClient>();
        }
    }
}
