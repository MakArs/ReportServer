using Autofac;
using Autofac.Extensions.DependencyInjection;
using AutoMapper;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using ReportService.Api.Models;

namespace ReportService.Api
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                    .UseServiceProviderFactory(new AutofacServiceProviderFactory())
                    .ConfigureContainer<ContainerBuilder>(builder =>
                    {
                        builder.RegisterType<ModelsMapperProfile>().As<Profile>().SingleInstance();

                        var boots = new Bootstrapper();
                        boots.ConfigureContainer(builder);
                    })
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder
                    .UseStartup<Startup>();
                });
    }
}