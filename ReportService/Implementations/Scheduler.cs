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

        private Task _workTask;
        private bool _started = false;
        private CancellationTokenSource cancelSource = new CancellationTokenSource();
        private CancellationToken cancelToken;

        private void WorkCycle()
        {
            while (_started)
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
            _started = true;
            cancelToken = cancelSource.Token;

            _workTask = new Task(WorkCycle, cancelToken);
            _workTask.Start();
        }

        public void OnStop()
        {
            _started = false;
            Task.Delay(1000).Wait();

            if (!_workTask.IsCanceled)
                cancelSource.Cancel();

        }
    }
}
