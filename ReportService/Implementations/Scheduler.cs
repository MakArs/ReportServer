using System;
using System.Threading;
using System.Threading.Tasks;

namespace ReportService.Implementations
{
    public class Scheduler
    {
        public Scheduler() { }

        public int Period { get; set; } = 60; // in seconds
        public Action TaskMethod { get; set; } = null; // may be with exceptions

        private Task workTask_;
        private bool started_ = false;
        private CancellationTokenSource cancelSource = new CancellationTokenSource();
        private CancellationToken cancelToken;

        private void WorkCycle()
        {
            while (started_)
            {
                try
                {
                    if (TaskMethod != null)
                        Task.Factory.StartNew(TaskMethod);
                }
                catch { }

                Task.Delay(Period * 1000).Wait();
            }
        }

        public void OnStart()
        {
            started_ = true;
            cancelToken = cancelSource.Token;

            workTask_ = new Task(WorkCycle, cancelToken);
            workTask_.Start();
        }

        public void OnStop()
        {
            started_ = false;
            Task.Delay(1000).Wait();

            if (!workTask_.IsCanceled)
                cancelSource.Cancel();

        }
    }
}
