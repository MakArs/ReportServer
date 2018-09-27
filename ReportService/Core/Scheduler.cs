using System;
using System.Threading;
using System.Threading.Tasks;

namespace ReportService.Core
{
    public class Scheduler
    {
        public int Period { get; set; } = 60; // in seconds
        public Action TaskMethod { get; set; } // may be with exceptions

        private Task workTask;
        private bool started;
        private readonly CancellationTokenSource cancelSource = new CancellationTokenSource();
        private CancellationToken cancelToken;

        private void WorkCycle()
        {
            while (started)
            {
                try
                {
                    if (TaskMethod != null)
                        Task.Factory.StartNew(TaskMethod, cancelToken);
                }

                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }

                Task.Delay(Period * 1000, cancelToken).Wait(cancelToken);
            }
        }

        public void OnStart()
        {
            started = true;
            cancelToken = cancelSource.Token;

            workTask = new Task(WorkCycle, cancelToken);
            workTask.Start();
        }

        public void OnStop()
        {
            started = false;
            Task.Delay(1000, cancelToken).Wait(cancelToken);

            if (!workTask.IsCanceled)
                cancelSource.Cancel();
        }
    }
}
