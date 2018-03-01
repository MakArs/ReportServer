using System;
using System.Collections.Generic;
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

        private Scheduler UpdateConfigScheduler;
        private Scheduler CheckScheduleAndExecuteScheduler;

        public Logic(ILifetimeScope aAutofac, IConfig config)
        {
            autofac_ = aAutofac;
            config_ = config;
            tasks_ = new List<RTask>();
            UpdateConfigScheduler = new Scheduler() { TaskMethod = UpdateTaskList };
            CheckScheduleAndExecuteScheduler = new Scheduler() { Period = 1, TaskMethod = CheckScheduleAndExecute };
        }

        private void UpdateTaskList()
        {
            config_.Reload();

            lock (this)
            {
                tasks_.Clear();

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
            }//lock
        }

        public string ForceExecute(int aTaskID, string aMail)
        {
            List<RTask> tasks;

            UpdateTaskList();
            string executed = "";
            lock (this)
                tasks = tasks_.ToList();

            foreach (var task in tasks_)
                if (task.ID == aTaskID)
                {
                    task.Execute(aMail);
                    executed += $"#{task.ID} "; //тут выбор,либо выполняется асинхронно и выдаёт надпись про отправку до отправления,либо так.
                    return executed;
                }
            return executed;
        }

        private void CheckScheduleAndExecute()
        {
            List<RTask> tasks;

            lock (this)
                tasks = tasks_.ToList();

            DateTime time = DateTime.Now;
            CultureInfo.CurrentCulture = new CultureInfo("en-US");
            var currentDay = time.ToString("ddd").ToLower().Substring(0, 2);
            var currentTime = time.ToString("HHmm");

            foreach (var task in tasks)
            {
                string[] schedDays = task.Schedule.Split(' ');

                if (schedDays.Any(s => s.Contains(currentDay) && s.Contains(currentTime)))
                {
                    Task.Factory.StartNew(()=>task.Execute()).ContinueWith(
                        _ => Console.WriteLine($"Task {task.ID} executed. Mail sent to {task.SendAddresses[0]}"));
                }
            }//for
        }
        
        public void Start()
        {
            UpdateConfigScheduler.OnStart();
            CheckScheduleAndExecuteScheduler.OnStart();
        }

        public void Stop()
        {
            UpdateConfigScheduler.OnStop();
            CheckScheduleAndExecuteScheduler.OnStop();
        }

    }
}
