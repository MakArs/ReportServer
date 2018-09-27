using System;
using System.Reflection;
using Autofac;
using Monik.Common;
using Nancy.Hosting.Self;
using ReportService.Interfaces;

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

            var version = Assembly.GetExecutingAssembly().GetName().Version;
            stringVersion = $"v.{version.Major}.{version.Minor}.{version.Build} ";

            monik = Bootstrapper.Global.Resolve<IMonik>();
            monik.ApplicationInfo("HostHolder.ctor");
        }

        private static readonly HostConfiguration HostConfigs = new HostConfiguration()
        {
            UrlReservations  = new UrlReservations {CreateAutomatically = true},
            RewriteLocalhost = true
        };

        public void Start()
        {
            monik.ApplicationWarning(stringVersion+"Started");

            try
            {
                nancyHost.Start();
            }
            catch (Exception e)
            {
                monik.ApplicationError(e.Message);
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
            }

            monik.ApplicationWarning(stringVersion+"Stopped");
            monik.OnStop();
        }
    }
}
