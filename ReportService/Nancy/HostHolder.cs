using System;
using Autofac;
using Monik.Client;
using Nancy.Hosting.Self;
using ReportService.Interfaces;

namespace ReportService.Nancy
{
    public class HostHolder : IHostHolder
    {
        private readonly IClientControl _monik;
        private readonly NancyHost      _nancyHost;

        public HostHolder()
        {
            _nancyHost = new NancyHost(
                new Uri("http://localhost:12345"),
                new Bootstrapper(),
                HostConfigs);

            _monik = Bootstrapper.Global.Resolve<IClientControl>();
            _monik.ApplicationInfo("HostHolder.ctor");
        }

        private static readonly HostConfiguration HostConfigs = new HostConfiguration()
        {
            UrlReservations  = new UrlReservations() {CreateAutomatically = true},
            RewriteLocalhost = true
        };

        public void Start()
        {
            _monik.ApplicationWarning("Started");

            try
            {
                _nancyHost.Start();
            }
            catch (Exception e)
            {
                _monik.ApplicationError(e.Message);
            }
        }

        public void Stop()
        {
            try
            {
                _nancyHost.Stop();
            }
            catch (Exception e)
            {
                _monik.ApplicationError(e.Message);
            }

            _monik.ApplicationWarning("Stopped");
            _monik.OnStop();
        }
    }
}
