using Autofac;

namespace ReportService.Extensions
{
    public static class ContainerBuilderExtensions
    {
        public static void RegisterImplementationSingleton<TInterface, TImplementation>(
            this ContainerBuilder builder)
        {
            builder
                .RegisterType<TImplementation>()
                .As<TInterface>()
                .SingleInstance();
        }

        public static void RegisterImplementation<TInterface, TImplementation>(
            this ContainerBuilder builder)
        {
            builder
                .RegisterType<TImplementation>()
                .As<TInterface>();
        }

        public static void RegisterInstance<TInterface, TImplementation>(
            this ContainerBuilder builder,
            TImplementation aInstance)
        {
            builder
                .Register(_ => aInstance)
                .As<TInterface>();
        }

        public static void RegisterSingleInstance<TInterface, TImplementation>(
            this ContainerBuilder builder,
            TImplementation aInstance)
        {
            builder
                .Register(_ => aInstance)
                .As<TInterface>()
                .SingleInstance();
        }

        public static void RegisterNamedSingleton<TInterface, TImplementation>(
            this ContainerBuilder builder,
            string name)
        {
            builder
                .RegisterType<TImplementation>()
                .Named<TInterface>(name)
                .SingleInstance();
        }

        public static void RegisterNamedImplementation<TInterface, TImplementation>(
            this ContainerBuilder builder,
            string name)
        {
            builder
                .RegisterType<TImplementation>()
                .Named<TInterface>(name);
        }
    }
}