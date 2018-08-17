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
        private bool started = false;
        private readonly CancellationTokenSource cancelSource = new CancellationTokenSource();
        private CancellationToken cancelToken;

        private void WorkCycle()
        {
            while (started)
            {
                try
                {
                    if (TaskMethod != null)
                        Task.Factory.StartNew(TaskMethod);
                }
                catch
                {
                }

                Task.Delay(Period * 1000).Wait();
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
            Task.Delay(1000).Wait();

            if (!workTask.IsCanceled)
                cancelSource.Cancel();

        }
    }
}
