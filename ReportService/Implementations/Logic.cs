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
        private readonly ILifetimeScope _autofac;
        private readonly IRepository _repository;
        private readonly IClientControl _monik;

        private List<RTask> _tasks;

        private readonly Scheduler _checkScheduleAndExecuteScheduler;

        public Logic(ILifetimeScope autofac, IRepository repository, IClientControl monik)
        {
            _autofac = autofac;
            _repository = repository;
            _tasks = new List<RTask>();
            _checkScheduleAndExecuteScheduler = new Scheduler() {Period = 60, TaskMethod = CheckScheduleAndExecute};
            _monik = monik;
        }

        private void UpdateTaskList()
        {
            lock (this)
            {
                _tasks.Clear();

                foreach (var dtoTask in _repository.GetTasks())
                {
                    var task = _autofac.Resolve<IRTask>(
                        new NamedParameter("id", dtoTask.Id),
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

        public string ForceExecute(int taskId, string mail)
        {
            List<RTask> tasks;
            string executed = "";
            lock (this)
                tasks = _tasks.ToList();

            foreach (var task in _tasks)
                if (task.Id == taskId)
                {
                    _monik.ApplicationInfo($"Начинаем отсылку отчёта {task.Id} на адрес {mail}");

                    executed += $"#{task.Id} ";
                    Task.Factory.StartNew(() => task.Execute(mail));
                    return executed;
                }

            return executed;
        }

        private void CheckScheduleAndExecute()
        {
            List<RTask> tasks;
            UpdateTaskList();
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
                    foreach(var mail in task.SendAddresses)
                        _monik.ApplicationInfo($"Начинаем отсылку отчёта {task.Id} на адрес {mail}");

                    Task.Factory.StartNew(() => task.Execute()).ContinueWith(
                        _ => Console.WriteLine($"Task {task.Id} executed. Mail sent to {task.SendAddresses[0]}"));
                }
            }//for
        }

        public string GetTaskView()
        {
            IViewExecutor tableView = _autofac.ResolveNamed<IViewExecutor>("tableviewex");
            IDataExecutor dataEx = _autofac.ResolveNamed<IDataExecutor>("commondataex");
            return tableView.Execute("", dataEx.Execute("select * from task", 5));
        }

        public string GetInstancesView(int taskId)
        {
            IViewExecutor tableView = _autofac.ResolveNamed<IViewExecutor>("tableviewex");
            IDataExecutor dataEx = _autofac.ResolveNamed<IDataExecutor>("commondataex");
            return tableView.Execute("", dataEx.Execute($"select * from instance where taskid={taskId}", 5));
        }

        public void CreateBase(string connStr)
        {
            _repository.CreateBase(connStr);
        }

        public void Start()
        {
            _checkScheduleAndExecuteScheduler.OnStart();
        }

        public void Stop()
        {
            _checkScheduleAndExecuteScheduler.OnStop();
        }

        public void UpdateTask(int taskId, RTask task)
        {
                var dtoTask = new DTO_Task()
                {
                    Id=taskId,
                    Schedule = task.Schedule,
                    ConnectionString = "",
                    ViewTemplate = task.ViewTemplate,
                    Query = task.Query,
                    SendAddress = String.Join(";", task.SendAddresses),
                    TryCount = task.TryCount,
                    QueryTimeOut = task.TimeOut,
                    TaskType = (int) task.Type
                };

                _repository.UpdateTask(taskId, dtoTask);
                UpdateTaskList();
        }

        public void DeleteTask(int taskId)
        {
            _repository.DeleteTask(taskId);
            UpdateTaskList();
        }

        public int CreateTask(RTask task)
        {
            var dtoTask = new DTO_Task()
            {
                Schedule = task.Schedule,
                ConnectionString = "",
                ViewTemplate = task.ViewTemplate,
                Query = task.Query,
                SendAddress = String.Join(";", task.SendAddresses),
                TryCount = task.TryCount,
                QueryTimeOut = task.TimeOut,
                TaskType = (int)task.Type
            };

            UpdateTaskList();

            return _repository.CreateTask(dtoTask);
        }
    }//class
}
