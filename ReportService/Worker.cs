using System;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Monik.Common;
using ReportService.Interfaces.Core;

namespace ReportService
{
    public class Worker : BackgroundService
    {
        public int Period { get; set; } = 60;
        private Action Method { get; set; }
        private readonly IMonik monik;
        private readonly string stringVersion;

        public Worker(IMonik monik, ILogic logic)
        {

            stringVersion = Assembly.GetExecutingAssembly().GetName().Version.ToString();

            Method = logic.CheckScheduleAndExecute;

            this.monik = monik;
            logic.Start();

            monik.ApplicationInfo("Worker.ctor");
            Console.WriteLine("HostHolder.ctor");
        }

        protected override async Task ExecuteAsync(CancellationToken cancelToken)
        {
            monik.ApplicationWarning(stringVersion + " Started");
            Console.WriteLine(stringVersion + " Started");

            while (!cancelToken.IsCancellationRequested)
            {
                try
                {
                    await Task.Factory.StartNew(Method, cancelToken);
                }
                catch (TaskCanceledException _)
                {
                    monik.ApplicationWarning("Stopping...");
                }
                catch (AggregateException aggrExc)
                {
                    monik.ApplicationError("Aggregate exception at: " + stringVersion);
                    aggrExc.Handle((ex) =>
                    {
                        monik.ApplicationError($"Inner exception of type {ex.GetType().Name} has occurred. Message: {ex.Message} Stacktrace: {ex.StackTrace}");
                        return true;
                    });
                }
                catch (Exception ex)
                {
                    monik.ApplicationError($"Error in core schedule loop: {ex}");
                }
                finally
                {
                    await Task.Delay(Period * 1000, cancelToken);
                }
            }
        }

        public override async Task StopAsync(CancellationToken cancelToken)
        {
            monik.ApplicationWarning(stringVersion + " Stopped");
            Console.WriteLine(stringVersion + " Stopped");

            await Task.Delay(2000, cancelToken);
        }
    }
}
