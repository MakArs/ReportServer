using ReportService.Nancy;
using Topshelf;

namespace ReportService
{
    class Program
    {
        static void Main(string[] args)
        {
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
                hostConfigurator.SetDisplayName("ReportServer");
                hostConfigurator.SetServiceName("ReportServer");
            });
        }
    }
}
