﻿using Autofac;
using ReportService.Implementations;
using ReportService.Interfaces;

namespace ReportService
{
    class BootsTrap
    {
        public static IContainer Container { get; set; }

        public static void Init()
        {
            var builder = new ContainerBuilder();
            builder.RegisterType<ConfigTest>()
                .As<IConfig>()
                .SingleInstance();

            builder.RegisterType<DataExecutorTest>()
                .As<IDataExecutor>()
                .SingleInstance();

            builder.RegisterType<ViewExecutor>()
                .As<IViewExecutor>()
                .SingleInstance();

            builder.RegisterType<PostMasterWork>()
                .As<IPostMaster>()
                .SingleInstance();

            builder.RegisterType<HostHolder>()
                .As<IHostHolder>()
                .SingleInstance();

            builder.RegisterType<Logic>()
                .As<ILogic>()
                .SingleInstance();

            builder.RegisterType<RTask>()
                .As<IRTask>();

            builder.Register(c => Container);

            Container = builder.Build();
        }
    }
}
