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
        //private IHostHolder holder_;
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

        public string ForceExecute(int aTaskIDs)
        {
            UpdateTaskList();
            string executed = "";
            foreach (RTask task in tasks_.Where(t => aTaskIDs == t.ID))
            {
                task.Execute();
                executed += $"#{task.ID} ";
            }

            return executed;
        }

        private void WorkCycleMethod()
        {
            UpdateTaskList();

            Stopwatch stepTimer = new Stopwatch();
            Stopwatch sumTimer = new Stopwatch();
            double reloadTrigger = 0;
            stepTimer.Start();
            sumTimer.Start();
            int i = 1;

            while (working_)
            {
                try
                {
                    Task.Delay(TimeSpan.FromSeconds(1)).Wait();
                    CultureInfo.CurrentCulture = new CultureInfo("en-US");
                    Console.WriteLine($"Step {i}. Passed from previous step: {stepTimer.Elapsed} seconds. " +
                        $"Total time passed: {sumTimer.Elapsed}. " +
                        $"Today: {new string(DateTime.Now.AddDays(i).ToString("ddd").ToLower().Take(2).ToArray())}");
                    stepTimer.Restart();

                    if (sumTimer.ElapsedMilliseconds / 1000 - reloadTrigger > 60)
                    {
                        UpdateTaskList();
                        reloadTrigger = sumTimer.ElapsedMilliseconds / 1000;
                    }

                    // TODO: schedule support
                    foreach (RTask task in tasks_)
                    {
                        string[] schedDays = task.Schedule.Split(' ');
                        if (schedDays.Any(s => s.Contains(new string(DateTime.Now.AddDays(i).ToString("ddd").ToLower().Take(2).ToArray()))))
                        {
                            if (1 == 1)
                            {
                                Task t = new  Task(() => task.Execute());//что-то подсказывает,что async делается по-другому
                                t.Start();
                            }
                        }
                    }
                    i++;
                }
                catch
                {
                    Task.Delay(100).Wait();
                }
            }//while
        }

        public void Start()
        {
            if (workTask_ != null)
                throw new Exception();

            workTask_ = new Task(WorkCycleMethod);
            workTask_.Start();
        }

        public void Stop()
        {
            working_ = false;

            //Task.Delay(1000).Wait();
            //if (!workCycle_.IsCanceled)
            // KILL
        }

    }
}
