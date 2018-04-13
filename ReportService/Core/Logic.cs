using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Autofac;
using AutoMapper;
using Monik.Client;
using Newtonsoft.Json;
using ReportService.Interfaces;
using ReportService.Nancy;

namespace ReportService.Core
{
    public class Logic : ILogic
    {
        private readonly ILifetimeScope _autofac;
        private readonly IRepository _repository;
        private readonly IClientControl _monik;
        private readonly IMapper _mapper;

        private readonly Scheduler _checkScheduleAndExecuteScheduler;
        private readonly List<RTask> _tasks;
        private readonly List<RSchedule> _schedules;
        private readonly List<RRecepientGroup> _recepientGroups;

        public Logic(ILifetimeScope autofac, IRepository repository, IClientControl monik, IMapper mapper)
        {
            _mapper = mapper;
            _autofac = autofac;
            _repository = repository;
            _tasks = new List<RTask>();
            _schedules = new List<RSchedule>();
            _recepientGroups = new List<RRecepientGroup>();
            _checkScheduleAndExecuteScheduler = new Scheduler() {Period = 60, TaskMethod = CheckScheduleAndExecute};
            _monik = monik;
        }

        private void UpdateScheduleList()
        {
            var schedList = _repository.GetAllSchedules();
            lock (this)
            {
                _schedules.Clear();
                foreach (var sched in schedList)
                {
                    _schedules.Add(_mapper.Map<RSchedule>(sched));
                }
            }
        }

        private void UpdateRecepientGroupsList()
        {
            var recepList = _repository.GetAllRecepientGroups();
            lock (this)
            {
                _recepientGroups.Clear();
                foreach (var sched in recepList)
                {
                    _recepientGroups.Add(_mapper.Map<RRecepientGroup>(sched));
                }
            }
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
                        new NamedParameter("schedule", _schedules
                            .FirstOrDefault(s => s.Id == dtoTask.ScheduleId)),
                        new NamedParameter("query", dtoTask.Query),
                        new NamedParameter("sendAddress", _recepientGroups
                            .FirstOrDefault(r => r.Id == dtoTask.RecepientGroupId)),
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
            lock (this)
                tasks = _tasks.ToList();

            var task = tasks.FirstOrDefault(t => t.Id == taskId);

            if (task == null) return "No tasks with such Id found..";
            _monik.ApplicationInfo($"Отсылка отчёта {task.Id} на адрес {mail} (ручной запуск)");

            Task.Factory.StartNew(() => task.Execute(mail));
            return $"Report {taskId} sent!";
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

            foreach (var task in tasks.Where(x => x.Schedule != null))
            {
                string[] schedDays = _schedules.First(s => s.Id == task.Schedule.Id).Schedule.Split(' ');

                if (!schedDays.Any(s => s.Contains(currentDay) && s.Contains(currentTime)))
                    continue;

                _monik.ApplicationInfo($"Отсылка отчёта {task.Id} по расписанию");

                Task.Factory.StartNew(() => task.Execute());
            } //for
        }

        public void CreateBase(string connStr)
        {
            try
            {
                _repository.CreateBase(connStr);
            }
            catch (Exception e)
            {
                _monik.ApplicationError(e.Message);
            }
        }

        public void Start()
        {
            UpdateScheduleList();
            UpdateRecepientGroupsList();
            UpdateTaskList();
            _checkScheduleAndExecuteScheduler.OnStart();
        }

        public void Stop()
        {
            _checkScheduleAndExecuteScheduler.OnStop();
        }

        public string GetTaskList_HtmlPage()
        {
            List<RTask> tasks;
            IViewExecutor tableView = _autofac.ResolveNamed<IViewExecutor>("tasklistviewex");
            lock (this)
                tasks = _tasks.ToList();

            var tasksView = tasks.Select(t => new
                {
                    t.Id,
                    SendAddresses = t.SendAddresses?.Addresses,
                    t.ViewTemplate,
                    Schedule = t.Schedule?.Name,
                    t.ConnectionString,
                    t.Query,
                    t.TryCount,
                    TimeOut = t.QueryTimeOut,
                    t.Type
                })
                .ToList();
            var jsonTasks = JsonConvert.SerializeObject(tasksView);
            return tableView.Execute("", jsonTasks);
        }

        public string GetInstanceList_HtmlPage(int taskId)
        {
            List<DtoInstance> instances = _repository.GetInstancesByTaskId(taskId);
            IViewExecutor tableView = _autofac.ResolveNamed<IViewExecutor>("instancelistviewex");
            var jsonInstances = JsonConvert.SerializeObject(instances);
            return tableView.Execute("", jsonInstances);
        }

        public void DeleteInstance(int instanceId)
        {
            _repository.DeleteInstance(instanceId);
            UpdateTaskList();
            _monik.ApplicationInfo($"Удалена запись {instanceId}");
        }

        public string GetAllInstanceCompactsByTaskIdJson(int taskId)
        {
            List<DtoInstanceCompact> instances = _repository.GetCompactInstancesByTaskId(taskId);
            return JsonConvert.SerializeObject(instances
                .Select(t => _mapper.Map<ApiInstanceCompact>(t)));
        }

        public string GetAllInstancesCompactJson()
        {
            return JsonConvert.SerializeObject(_repository.GetAllCompactInstances()
                .Select(t => _mapper.Map<ApiInstanceCompact>(t)));
        }

        public string GetInstanceByIdJson(int id)
        {
            return JsonConvert.SerializeObject(_mapper.Map<ApiInstance>(_repository.GetInstanceById(id)));
        }

        public string GetAllTaskCompactsJson()
        {
            List<RTask> tasks;
            lock (this)
                tasks = _tasks.ToList();
            var tr= JsonConvert.SerializeObject(tasks
                .Select(t => _mapper.Map<ApiTaskCompact>(t)));
            return tr;
        }

        public string GetTaskByIdJson(int id)
        {
            List<RTask> tasks;
            lock (this)
                tasks = _tasks.ToList();
            return JsonConvert.SerializeObject(_mapper.Map<ApiTask>(tasks.First(t => t.Id == id)));
        }

        public string GetAllSchedulesJson()
        {
            return JsonConvert.SerializeObject(_repository.GetAllSchedules()
                .Select(s=> _mapper.Map<ApiSchedule>(s))); 
        }

        public string GetAllRecepientGroupsJson()
        {
            return JsonConvert.SerializeObject(_repository.GetAllRecepientGroups()
                .Select(s => _mapper.Map<ApiRecepientGroup>(s)));
        }

        public string GetCurrentViewByTaskId(int taskId)
        {
            List<RTask> tasks;
            lock (this)
                tasks = _tasks.ToList();

            var task = tasks.FirstOrDefault(t => t.Id == taskId);

            if (task == null) return "No tasks with such Id found..";
            return task.GetCurrentView();
        }

        public void UpdateTask(ApiTask task)
        {
            var dtoTask = _mapper.Map<DtoTask>(task);
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
            var dtoTask = _mapper.Map<DtoTask>(task);
            var newTaskId = _repository.CreateTask(dtoTask);
            UpdateTaskList();
            _monik.ApplicationInfo($"Создана задача {newTaskId}");
            return newTaskId;
        }
    } //class
}
