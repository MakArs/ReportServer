using System;
using System.Reflection;
using Autofac;
using Monik.Common;
using Nancy.Hosting.Self;
using ReportService.Interfaces.Core;

namespace ReportService.Nancy
{
    public class HostHolder : IHostHolder
    {
        private readonly IMonik monik;
        private readonly NancyHost      nancyHost;
        private readonly string stringVersion;

        public HostHolder()
        {
            nancyHost = new NancyHost(
                new Uri("http://localhost:12345"),
                new Bootstrapper(),
                HostConfigs);

            stringVersion = Assembly.GetExecutingAssembly().GetName().Version.ToString();

            monik = Bootstrapper.Global.Resolve<IMonik>();
            monik.ApplicationInfo("HostHolder.ctor");
            Console.WriteLine("HostHolder.ctor");
        }

        private static readonly HostConfiguration HostConfigs = new HostConfiguration()
        {
            UrlReservations  = new UrlReservations {CreateAutomatically = true},
            RewriteLocalhost = true
        };

        public void Start()
        {
            monik.ApplicationWarning(stringVersion+" Started");
            Console.WriteLine(stringVersion + " Started");

            try
            {
                nancyHost.Start();
            }
            catch (Exception e)
            {
                monik.ApplicationError(e.Message);
                Console.WriteLine(e.Message);
            }
        }

        public void Stop()
        {
            try
            {
                nancyHost.Stop();
            }
            catch (Exception e)
            {
                monik.ApplicationError(e.Message);
                Console.WriteLine(e.Message);
            }

            monik.ApplicationWarning(stringVersion+" Stopped");
            Console.WriteLine(stringVersion + " Stopped");
            monik.OnStop();
        }
    }
}
