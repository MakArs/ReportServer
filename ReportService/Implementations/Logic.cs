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
                        new NamedParameter("template", dtoTask.ViewTemplate),
                        new NamedParameter("schedule", dtoTask.Schedule),
                        new NamedParameter("query", dtoTask.Query),
                        new NamedParameter("sendAddress", dtoTask.SendAddress),
                        new NamedParameter("tryCount", dtoTask.TryCount),
                        new NamedParameter("timeOut", dtoTask.QueryTimeOut),
                        new NamedParameter("taskType", (RTaskType)dtoTask.TaskType));

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

            foreach (var task in tasks)
                if (task.Id == taskId)
                {
                    _monik.ApplicationInfo($"Отсылка отчёта { task.Id} на адрес {mail} (ручной запуск)");

                    executed += $"#{task.Id} ";
                    Task.Factory.StartNew(() => task.Execute(mail));
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

                if (!schedDays.Any(s => s.Contains(currentDay) && s.Contains(currentTime))) continue;
                foreach (var mail in task.SendAddresses)
                {
                    _monik.ApplicationInfo($"Отсылка отчёта {task.Id} на адрес {mail} по расписанию");
                    Task.Factory.StartNew(() => task.Execute()).ContinueWith(
                        _ => _monik.ApplicationInfo($"Отчёт {task.Id} успешно отослан на адрес {mail}"));
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
            UpdateTaskList();
            _checkScheduleAndExecuteScheduler.OnStart();
        }

        public void Stop()
        {
            _checkScheduleAndExecuteScheduler.OnStop();
        }

        public void UpdateTask(int taskId, RTask task)
        {
                var dtoTask = new DTOTask()
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
            var dtoTask = new DTOTask()
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

            var newTaskId= _repository.CreateTask(dtoTask);
            UpdateTaskList();
            return newTaskId;
        }
    }//class
}
