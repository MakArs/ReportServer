using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Autofac;
using ReportService.Interfaces;

namespace ReportService.Implementations
{
    /* 
     * Functions:
     * 1. Task list prepare and update
     * 2. Control schedules
     * 3. Async run RTasks
     */
    public class Logic : ILogic
    {
        private IConfig config_;
        private List<RTask> tasks_;
        private ILifetimeScope autofac_;
        private bool working_ = true;
        private Task workTask_ = null;

        public Logic(ILifetimeScope aAutofac, IConfig config)
        {
            autofac_ = aAutofac;
            config_ = config;
            tasks_ = new List<RTask>();
        }

        private void UpdateTaskList()
        {
            tasks_.Clear();
            config_.Reload();

            foreach (var dto_task in config_.GetTasks())
            {
                var task = autofac_.Resolve<IRTask>(
                    new NamedParameter("ID", dto_task.ID),
                    new NamedParameter("aTemplate", dto_task.ViewTemplate),
                    new NamedParameter("aSchedule", dto_task.Schedule),
                    new NamedParameter("aQuery", dto_task.Query),
                    new NamedParameter("aSendAddress", dto_task.SendAddress),
                    new NamedParameter("aTryCount", dto_task.TryCount),
                    new NamedParameter("aTimeOut", dto_task.QueryTimeOut));

                tasks_.Add((RTask)task);
            }
        }

        public string ForceExecute(int aTaskIDs, string mail)
        {
            UpdateTaskList();
            string executed = "";
            foreach (RTask task in tasks_.Where(t => aTaskIDs == t.ID))
            {
                task.SendAddresses = new string[] { mail };
                task.ExecuteAsync();
                executed += $"#{task.ID} ";
            }

            return executed;
        }

        private async void WorkCycleMethodAsync()
        {
            Stopwatch stepTimer = new Stopwatch();
            Stopwatch sumTimer = new Stopwatch();
            stepTimer.Start();
            sumTimer.Start();
            int i = 1;

            while (working_)
            {
                DateTime time = DateTime.Now;
                var oneSecond = Task.Delay(TimeSpan.FromSeconds((60 - time.Second)));
                try
                {
                    CultureInfo.CurrentCulture = new CultureInfo("en-US");
                    Console.WriteLine($"Step {i}. Passed from previous step: {stepTimer.Elapsed} seconds. " +
                        $"Total time passed: {sumTimer.Elapsed}. " +
                        $"Today: {new string(DateTime.Now.AddDays(i).ToString("ddd").ToLower().Take(2).ToArray())}");
                    stepTimer.Restart();
                    UpdateTaskList();

                    foreach (RTask task in tasks_)
                    {
                        string[] schedDays = task.Schedule.Split(' ');
                        if (schedDays.Any(s => s.Contains(new string(time.AddDays(i).ToString("ddd").ToLower().Take(2).ToArray()))
                        && 1 == 1))// s.Contains(time.ToString("hhmm"))))
                            task.ExecuteAsync();
                    }
                    i++;

                }
                catch
                {
                    Task.Delay(100).Wait();
                }
                await oneSecond;
            }//while
        }

        public void Start()
        {
            if (workTask_ != null)
                throw new Exception();

            workTask_ = new Task(WorkCycleMethodAsync);
            workTask_.Start();
        }

        public void Stop()
        {
            working_ = false;
            // TODO: Task.Delay(1000).Wait();
            //if (!workCycle_.IsCanceled)
            // KILL
        }

    }
}
