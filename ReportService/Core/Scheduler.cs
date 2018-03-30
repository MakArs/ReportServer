using System;
using System.Threading;
using System.Threading.Tasks;

namespace ReportService.Core
{
    public class Scheduler
    {
        public Scheduler() { }

        public int Period { get; set; } = 60; // in seconds
        public Action TaskMethod { get; set; } = null; // may be with exceptions

        private Task _workTask;
        private bool _started = false;
        private readonly CancellationTokenSource _cancelSource = new CancellationTokenSource();
        private CancellationToken _cancelToken;

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
            _cancelToken = _cancelSource.Token;

            _workTask = new Task(WorkCycle, _cancelToken);
            _workTask.Start();
        }

        public void OnStop()
        {
            _started = false;
            Task.Delay(1000).Wait();

            if (!_workTask.IsCanceled)
                _cancelSource.Cancel();

        }
    }
}
