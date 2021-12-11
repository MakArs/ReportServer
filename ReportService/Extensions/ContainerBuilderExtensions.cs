using Autofac;
using ReportService.Interfaces.Core;
using ReportService.Interfaces.Operations;

namespace ReportService.Extensions
{
    public static class ContainerBuilderExtensions
    {
        public static void RegisterImplementationSingleton<TInterface, TImplementation>(this ContainerBuilder builder)
        {
            builder.RegisterType<TImplementation>()
                .As<TInterface>()
                .SingleInstance();
        }

        public static void RegisterImplementation<TInterface, TImplementation>(this ContainerBuilder builder)
        {
            builder.RegisterType<TImplementation>()
                .As<TInterface>();
        }

        public static void RegisterInstance<TInterface, TImplementation>(this ContainerBuilder builder, TImplementation aInstance)
        {
            builder.Register(_ => aInstance)
                .As<TInterface>();
        }

        public static void RegisterSingleInstance<TInterface, TImplementation>(this ContainerBuilder builder, TImplementation aInstance)
        {
            builder.Register(_ => aInstance)
                .As<TInterface>()
                .SingleInstance();
        }

        public static void RegisterNamedSingleton<TInterface, TImplementation>(this ContainerBuilder builder, string name)
        {
            builder.RegisterType<TImplementation>()
                .Named<TInterface>(name)
                .SingleInstance();
        }

        public static void RegisterNamedImplementation<TInterface, TImplementation>(this ContainerBuilder builder, string name)
        {
            builder.RegisterType<TImplementation>()
                .Named<TInterface>(name);
        }

        public static void RegisterNamedDataExporter<TImplementation, TConfigType>(this ContainerBuilder builder, string name)
            where TImplementation : IOperation
            where TConfigType : IExporterConfig
        {
            builder.RegisterType<TImplementation>()
                .Named<IOperation>(name)
                .Keyed<IOperation>(typeof(TConfigType));
        }

        public static void RegisterNamedDataImporter<TImplementation, TConfigType>(this ContainerBuilder builder, string name)
            where TImplementation : IOperation
            where TConfigType : IImporterConfig
        {
            builder.RegisterType<TImplementation>()
                .Named<IOperation>(name)
                .Keyed<IOperation>(typeof(TConfigType));
        }

        public static void RegisterNamedViewExecutor<TImplementation>(this ContainerBuilder builder, string name) 
            where TImplementation : IViewExecutor
        {
            builder.RegisterType<TImplementation>()
                .Named<IViewExecutor>(name);
        }
    }
}
