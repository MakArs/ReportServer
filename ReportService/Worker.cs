using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Monik.Common;
using ReportService.Entities;

namespace ReportService
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> logger;
        public int Period { get; set; } = 3;
        private Action Method { get; set; }
        private readonly IMonik monik;

        public Worker(ILogger<Worker> logger, ThreadSafeRandom rnd, IMonik monik)
        {
            var yup = 0;
            this.logger = logger;
            Method = () => Console.WriteLine($"Cycle:{yup++}");
            monik.ApplicationInfo("Test message 2548");
            this.monik = monik;
        }

        protected override async Task ExecuteAsync(CancellationToken cancelToken)
        {
            var yap = 1;
            while (!cancelToken.IsCancellationRequested)
            {
                monik.ApplicationInfo($"Test message {2549+yap++}");

                logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);

                await Task.Factory.StartNew(Method, cancelToken);

                await Task.Delay(Period * 1000, cancelToken);
            }
        }
    }
}
