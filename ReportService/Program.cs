using ReportService.Implementations;
using System;
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

            HostFactory.Run(x =>
            {
                x.Service<HostHolder>(s =>
                {
                    s.ConstructUsing(name => new HostHolder());
                    s.WhenStarted(tc => tc.Start());
                    s.WhenStopped(tc => tc.Stop());
                });

                x.RunAsLocalSystem();
                x.SetDescription("ReportServer service");
                x.SetDisplayName("ReportServer");
                x.SetServiceName("ReportServer");
            });
        }
    }
}
