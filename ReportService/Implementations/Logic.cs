using System;
using System.Collections.Generic;
using System.Diagnostics;
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
        private IConfig _config;
        public List<RTask> _tasks { get; }
        private ILifetimeScope _autofac;

        private Scheduler UpdateConfigScheduler;
        private Scheduler CheckScheduleAndExecuteScheduler;
        public readonly IClientControl Monik;

        public Logic(ILifetimeScope aAutofac, IConfig config, IClientControl monik)
        {
            _autofac = aAutofac;
            _config = config;
            _tasks = new List<RTask>();
            UpdateConfigScheduler = new Scheduler() { TaskMethod = UpdateTaskList };
            CheckScheduleAndExecuteScheduler = new Scheduler() { TaskMethod = CheckScheduleAndExecute };
            Monik = monik;
        }

        private void UpdateTaskList()
        {
            _config.Reload();

            lock (this)
            {
                _tasks.Clear();

                foreach (var dtoTask in _config.GetTasks())
                {
                    var task = _autofac.Resolve<IRTask>(
                        new NamedParameter("ID", dtoTask.ID),
                        new NamedParameter("aTemplate", dtoTask.ViewTemplate),
                        new NamedParameter("aSchedule", dtoTask.Schedule),
                        new NamedParameter("aQuery", dtoTask.Query),
                        new NamedParameter("aSendAddress", dtoTask.SendAddress),
                        new NamedParameter("aTryCount", dtoTask.TryCount),
                        new NamedParameter("aTimeOut", dtoTask.QueryTimeOut),
                        new NamedParameter("aTaskType", (RTaskType)dtoTask.TaskType));

                    _tasks.Add((RTask)task);
                }
            }//lock
        }

        public string ForceExecute(int aTaskID, string aMail)
        {
            List<RTask> tasks;

            UpdateTaskList();
            string executed = "";
            lock (this)
                tasks = _tasks.ToList();

            foreach (var task in _tasks)
                if (task.ID == aTaskID)
                {
                    Monik.ApplicationInfo($"Начинаем отсылку отчёта {task.ID} на адрес {aMail}");
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
                tasks = _tasks.ToList();

            DateTime time = DateTime.Now;
            CultureInfo.CurrentCulture = new CultureInfo("en-US");
            var currentDay = time.ToString("ddd").ToLower().Substring(0, 2);
            var currentTime = time.ToString("HHmm");

            foreach (var task in tasks)
            {
                string[] schedDays = task.Schedule.Split(' ');

                if (schedDays.Any(s => s.Contains(currentDay) && s.Contains(currentTime)))
                {
                    foreach(var mail in task.SendAddresses) Monik.ApplicationInfo($"Начинаем отсылку отчёта {task.ID} на адрес {mail}");
                    Task.Factory.StartNew(() => task.Execute()).ContinueWith(
                        _ => Console.WriteLine($"Task {task.ID} executed. Mail sent to {task.SendAddresses[0]}"));
                }
            }//for
        }

        public string GetTaskView()
        {
            IViewExecutor tableView = _autofac.ResolveNamed<IViewExecutor>("tableviewex");
            IDataExecutor dataEx = _autofac.ResolveNamed<IDataExecutor>("commondataex");
            return tableView.Execute("", dataEx.Execute("select * from task", 5));
        }

        public string GetInstancesView(int ataskID)
        {
            IViewExecutor tableView = _autofac.ResolveNamed<IViewExecutor>("tableviewex");
            IDataExecutor dataEx = _autofac.ResolveNamed<IDataExecutor>("commondataex");
            return tableView.Execute("", dataEx.Execute($"select * from instance where taskid={ataskID}", 5));
        }

        public void CreateBase(string aconnstr)
        {
            _config.CreateBase(aconnstr);
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

        public void UpdateTask(int ataskID, RTask atask)
        {
            var dtoTask = new DTO_Task() {
                Schedule =atask.Schedule,
                ConnectionString ="",
                ViewTemplate = atask.ViewTemplate,
                Query = atask.Query,
                SendAddress = String.Join(";",atask.SendAddresses),
                TryCount = atask.TryCount,
                QueryTimeOut = atask.TimeOut,
                TaskType = (int)atask.Type};

            _config.UpdateTask(ataskID, dtoTask);
        }

        public void DeleteTask(int ataskID)
        {
            _config.DeleteTask(ataskID);
        }

        public int CreateTask(RTask atask)
        {
            var dtoTask = new DTO_Task()
            {
                Schedule = atask.Schedule,
                ConnectionString = "",
                ViewTemplate = atask.ViewTemplate,
                Query = atask.Query,
                SendAddress = String.Join(";", atask.SendAddresses),
                TryCount = atask.TryCount,
                QueryTimeOut = atask.TimeOut,
                TaskType = (int)atask.Type
            };
            return _config.CreateTask(dtoTask);
        }
    }
}
