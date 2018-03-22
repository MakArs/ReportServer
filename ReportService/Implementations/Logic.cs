using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Autofac;
using Monik.Client;
using ReportService.Interfaces;

namespace ReportService.Implementations
{
    /* 
     * Functions:
     * 1. Task list prepare and update
     * 2. Control schedules
     * 3. Async run RTasks
     * 4. Get Task&Instance html results
     */
    public class Logic : ILogic
    {
        private IConfig config_;
        private List<RTask> tasks_;
        private ILifetimeScope autofac_;

        private Scheduler UpdateConfigScheduler;
        private Scheduler CheckScheduleAndExecuteScheduler;
        private readonly IClientControl _monik;

        public Logic(ILifetimeScope aAutofac, IConfig config)
        {
            autofac_ = aAutofac;
            config_ = config;
            tasks_ = new List<RTask>();
            UpdateConfigScheduler = new Scheduler() { TaskMethod = UpdateTaskList };
            CheckScheduleAndExecuteScheduler = new Scheduler() { TaskMethod = CheckScheduleAndExecute };
            _monik = aAutofac.Resolve<IClientControl>();
        }

        private void UpdateTaskList()
        {
            config_.Reload();

            lock (this)
            {
                tasks_.Clear();

                foreach (var dtoTask in config_.GetTasks())
                {
                    var task = autofac_.Resolve<IRTask>(
                        new NamedParameter("ID", dtoTask.ID),
                        new NamedParameter("aTemplate", dtoTask.ViewTemplate),
                        new NamedParameter("aSchedule", dtoTask.Schedule),
                        new NamedParameter("aQuery", dtoTask.Query),
                        new NamedParameter("aSendAddress", dtoTask.SendAddress),
                        new NamedParameter("aTryCount", dtoTask.TryCount),
                        new NamedParameter("aTimeOut", dtoTask.QueryTimeOut),
                        new NamedParameter("aTaskType", (RTaskType)dtoTask.TaskType));

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
                    _monik.ApplicationInfo($"Начинаем отсылку отчёта {task.ID} на адрес {aMail}");
                    executed += $"#{task.ID} ";
                    Task.Factory.StartNew(() => task.Execute(aMail));
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
                    foreach(var mail in task.SendAddresses) _monik.ApplicationInfo($"Начинаем отсылку отчёта {task.ID} на адрес {mail}");
                    Task.Factory.StartNew(() => task.Execute()).ContinueWith(
                        _ => Console.WriteLine($"Task {task.ID} executed. Mail sent to {task.SendAddresses[0]}"));
                }
            }//for
        }

        public string GetTaskView()
        {
            IViewExecutor tableView = autofac_.ResolveNamed<IViewExecutor>("tableviewex");
            IDataExecutor dataEx = autofac_.ResolveNamed<IDataExecutor>("commondataex");
            return tableView.Execute("", dataEx.Execute("select * from task", 5));
        }

        public string GetInstancesView(int ataskID)
        {
            IViewExecutor tableView = autofac_.ResolveNamed<IViewExecutor>("tableviewex");
            IDataExecutor dataEx = autofac_.ResolveNamed<IDataExecutor>("commondataex");
            return tableView.Execute("", dataEx.Execute($"select * from instance where taskid={ataskID}", 5));
        }

        public void CreateBase(string aconnstr)
        {
            config_.CreateBase(aconnstr);
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
