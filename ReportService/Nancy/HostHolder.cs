using System;
using Autofac;
using Monik.Client;
using Monik.Common;
using Nancy.Hosting.Self;
using ReportService.Interfaces;

namespace ReportService.Nancy
{
    public class HostHolder : IHostHolder
    {
        private readonly IMonik monik;
        private readonly NancyHost      nancyHost;

        public HostHolder()
        {
            nancyHost = new NancyHost(
                new Uri("http://localhost:12345"),
                new Bootstrapper(),
                HostConfigs);

            monik = Bootstrapper.Global.Resolve<IMonik>();
            monik.ApplicationInfo("HostHolder.ctor");
        }

        private static readonly HostConfiguration HostConfigs = new HostConfiguration()
        {
            UrlReservations  = new UrlReservations() {CreateAutomatically = true},
            RewriteLocalhost = true
        };

        public void Start()
        {
            monik.ApplicationWarning("Started");

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

            monik.ApplicationWarning("Stopped");
            monik.OnStop();
        }
    }
}
