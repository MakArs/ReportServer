using System;
using System.IO;
using System.Reflection;
using ReportService.Implementations;
using Topshelf;

namespace ReportService
{
    class Program
    {
        static void Main(string[] args)
        {
            //BootsTrap.Init();
            //var cont = BootsTrap.Container;
            //ILogic log = cont.Resolve<ILogic>();
            //log.Execute();

            //HostHolder hld = new HostHolder();
            //hld.Start();

            //Console.ReadLine();
            //hld.Stop();
            HostFactory.Run(hostConfigurator =>
            {
                hostConfigurator.Service<HostHolder>(serviceConfigurator =>
                {
                    serviceConfigurator.ConstructUsing(name => new HostHolder());
                    serviceConfigurator.WhenStarted(hostHolder => hostHolder.Start());
                    serviceConfigurator.WhenStopped(hostHolder => hostHolder.Stop());
                });

                hostConfigurator.RunAsLocalSystem();
                //System.IO.Directory.SetCurrentDirectory(System.AppDomain.CurrentDomain.BaseDirectory);
                hostConfigurator.SetDescription("ReportServer service");
                hostConfigurator.SetDisplayName("ReportServerr");
                hostConfigurator.SetServiceName("ReportServer");
            });
        }
    }
}
