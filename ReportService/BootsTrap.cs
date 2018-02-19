using Autofac;
using ReportService.Interfaces;
using ReportService.Models;

namespace ReportService
{
    class BootsTrap
    {
        public static IContainer Container { get; set; }

        public static void Init()
        {
            var builder = new ContainerBuilder();
            builder.RegisterType<Config>()
                .As<IConfig>()
                .SingleInstance();

            builder.RegisterType<DataExecutor>()
                .As<IDataExecutor>()
                .SingleInstance();

            builder.RegisterType<ViewExecutor>()
                .As<IViewExecutor>()
                .SingleInstance();

            builder.RegisterType<Logic>()
                .As<ILogic>()
                .SingleInstance();

            Container = builder.Build();
        }
    }
}
