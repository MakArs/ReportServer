using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Autofac;
using AutoMapper;
using Monik.Client;
using Newtonsoft.Json;
using ReportService.Implementations;
using ReportService.Interfaces;
using ReportService.Nancy;

namespace ReportService.Core
{
    public class Logic : ILogic
    {
        private readonly ILifetimeScope _autofac;
        private readonly IRepository _repository;
        private readonly IClientControl _monik;
        private readonly Scheduler _checkScheduleAndExecuteScheduler;
        private readonly List<RTask> _tasks;
        private readonly IMapper _mapper;

        public Logic(ILifetimeScope autofac, IRepository repository, IClientControl monik, IMapper mapper)
        {
            _autofac = autofac;
            _repository = repository;
            _tasks = new List<RTask>();
            _checkScheduleAndExecuteScheduler = new Scheduler() {Period = 60, TaskMethod = CheckScheduleAndExecute};
            _monik = monik;
            _mapper = mapper;
        }

        private void UpdateTaskList()
        {
            var taskLst = _repository.GetTasks();

            lock (this)
            {
                _tasks.Clear();

                foreach (var dtoTask in taskLst)
                {
                    var task = _autofac.Resolve<IRTask>(
                        new NamedParameter("id", dtoTask.Id),
                        new NamedParameter("template", dtoTask.ViewTemplate),
                        new NamedParameter("schedule", dtoTask.Schedule),
                        new NamedParameter("query", dtoTask.Query),
                        new NamedParameter("sendAddress", dtoTask.SendAddresses),
                        new NamedParameter("tryCount", dtoTask.TryCount),
                        new NamedParameter("timeOut", dtoTask.QueryTimeOut),
                        new NamedParameter("taskType", (RTaskType) dtoTask.TaskType),
                        new NamedParameter("connStr", dtoTask.ConnectionString));

                    _tasks.Add((RTask) task);
                }
            } //lock
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
                    _monik.ApplicationInfo($"Отсылка отчёта {task.Id} на адрес {mail} (ручной запуск)");

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
                Task.Factory.StartNew(() => task.Execute());
            } //for
        }

        public string GetTaskList_HtmlPage()
        {
            List<RTask> tasks;
            IViewExecutor tableView = _autofac.ResolveNamed<IViewExecutor>("tasklistviewex");
            lock (this)
                tasks = _tasks.ToList();
            var jsonTasks = JsonConvert.SerializeObject(tasks);
            return tableView.Execute("", jsonTasks);
        }

        public string GetInstanceList_HtmlPage(int taskId)
        {
            List<DTOInstance> instances = _repository.GetInstances(taskId);
            IViewExecutor tableView = _autofac.ResolveNamed<IViewExecutor>("instancelistviewex");
            var jsonInstances = JsonConvert.SerializeObject(instances);
            return tableView.Execute("", jsonInstances);
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

        public void UpdateTask(ApiTask task)
        {
            var dtoTask = _mapper.Map<ApiTask, DTOTask>(task);
            _repository.UpdateTask(dtoTask);
            UpdateTaskList();
            _monik.ApplicationInfo($"Обновлена задача {task.Id}");
        }

        public void DeleteTask(int taskId)
        {
            _repository.DeleteTask(taskId);
            UpdateTaskList();
            _monik.ApplicationInfo($"Удалена задача {taskId}");
        }

        public int CreateTask(ApiTask task)
        {
            var dtoTask = _mapper.Map<ApiTask, DTOTask>(task);
            var newTaskId = _repository.CreateTask(dtoTask);
            UpdateTaskList();
            _monik.ApplicationInfo($"Создана задача {newTaskId}");
            return newTaskId;
        }
    } //class
}
