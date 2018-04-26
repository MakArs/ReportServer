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
        private readonly List<DtoSchedule> _schedules;
        private readonly List<RRecepientGroup> _recepientGroups;
        private readonly List<DtoReport> _reports;

        public Logic(ILifetimeScope autofac, IRepository repository, IClientControl monik, IMapper mapper)
        {
            _mapper = mapper;
            _autofac = autofac;
            _repository = repository;
            _tasks = new List<RTask>();
            _schedules = new List<DtoSchedule>();
            _recepientGroups = new List<RRecepientGroup>();
            _reports=new List<DtoReport>();
            _checkScheduleAndExecuteScheduler = new Scheduler {Period = 60, TaskMethod = CheckScheduleAndExecute};
            _monik = monik;
        }//ctor

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

        private void UpdateScheduleList()
        {
            var schedList = _repository.GetAllSchedules();
            lock (this)
            {
                _schedules.Clear();
                foreach (var sched in schedList)
                    _schedules.Add(sched);
            }
        }

        private void UpdateReportsList()
        {
            var repList = _repository.GetAllReports();
            lock (this)
            {
                _reports.Clear();
                foreach (var rep in repList)
                    _reports.Add(rep);
            }
        }

        private void UpdateTaskList()
        {
            var taskLst = _repository.GetAllTasks();
            lock (this)
            {
                _tasks.Clear();

                foreach (var dtoTask in taskLst)
                {
                    var report = _reports.First(rep => rep.Id == dtoTask.ReportId);
                    var task = _autofac.Resolve<IRTask>(
                        new NamedParameter("id", dtoTask.Id),
                        new NamedParameter("template", report.ViewTemplate),
                        new NamedParameter("schedule", _schedules
                            .FirstOrDefault(s => s.Id == dtoTask.ScheduleId)),
                        new NamedParameter("query", report.Query),
                        new NamedParameter("sendAddress", _recepientGroups
                            .FirstOrDefault(r => r.Id == dtoTask.RecepientGroupId)),
                        new NamedParameter("tryCount", dtoTask.TryCount),
                        new NamedParameter("timeOut", report.QueryTimeOut),
                        new NamedParameter("reportType", (RReportType)report.ReportType),
                        new NamedParameter("connStr", report.ConnectionString),
                        new NamedParameter("reportId", report.Id));

                    _tasks.Add((RTask)task);
                }
            } //lock
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

        private void CreateBase(string connStr)
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
            //CreateBase(ConfigurationManager.AppSettings["DBConnStr"]);
            UpdateScheduleList();
            UpdateRecepientGroupsList();
            UpdateReportsList();
            UpdateTaskList();
            _checkScheduleAndExecuteScheduler.OnStart();
        }

        public void Stop()
        {
            _checkScheduleAndExecuteScheduler.OnStop();
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

        public string GetFullInstanceList_HtmlPage(int taskId)
        {
            List<DtoFullInstance> instances = _repository.GetFullInstancesByTaskId(taskId);
            IViewExecutor tableView = _autofac.ResolveNamed<IViewExecutor>("instancelistviewex");
            var jsonInstances = JsonConvert.SerializeObject(instances);
            return tableView.Execute("", jsonInstances);
        }

        public string GetAllTasksJson()
        { //todo:test
            List<RTask> tasks;
            lock (this)
                tasks = _tasks.ToList();
            var tr= JsonConvert.SerializeObject(tasks
                .Select(t => _mapper.Map<ApiTask>(t)));
            return tr;
        }

        public string GetFullTaskByIdJson(int id)
        {//todo:test
            List<RTask> tasks;
            lock (this)
                tasks = _tasks.ToList();
            return JsonConvert.SerializeObject(_mapper.Map<ApiFullTask>(tasks.First(t => t.Id == id)));
        }

        public void DeleteTask(int taskId)
        {//todo:test
            _repository.DeleteTask(taskId);
            UpdateTaskList();
            _monik.ApplicationInfo($"Удалена задача {taskId}");
        }

        public int CreateTask(ApiFullTask task)
        {//todo:test
            if (task.ReportId>0)
            {
                var dtoTask = _mapper.Map<DtoTask>(task);
                var newTaskId = _repository.CreateEntity(dtoTask);
                UpdateTaskList();
                _monik.ApplicationInfo($"Создана задача {newTaskId}");
                return newTaskId;
            }
            else
            {
                task.ReportId = _repository.CreateEntity(_mapper.Map<DtoReport>(task));
                var dtoTask = _mapper.Map<DtoTask>(task);
                var newTaskId = _repository.CreateEntity(dtoTask);
                UpdateReportsList();
                UpdateTaskList();
                _monik.ApplicationInfo($"Создана задача {newTaskId}");
                return newTaskId;
            }
        }

        public void UpdateTask(ApiFullTask task)
        {//todo:test
            if (task.ReportId > 0)
            {
                var dtoTask = _mapper.Map<DtoTask>(task);
                _repository.UpdateEntity(dtoTask);
                UpdateTaskList();
                _monik.ApplicationInfo($"Обновлена задача {task.Id}");
            }
            else
            {
                task.ReportId = _repository.CreateEntity(_mapper.Map<DtoReport>(task));
                var dtoTask = _mapper.Map<DtoTask>(task);
                _repository.UpdateEntity(dtoTask);
                UpdateReportsList();
                UpdateTaskList();
                _monik.ApplicationInfo($"Обновлена задача {task.Id}");
            }
        }

        public string GetAllInstancesJson()
        {//todo:test
            return JsonConvert.SerializeObject(_repository.GetAllInstances());
        }

        public string GetAllInstancesByTaskIdJson(int taskId)
        {//todo:test
            return JsonConvert.SerializeObject(_repository.GetInstancesByTaskId(taskId));
        }

        public string GetFullInstanceByIdJson(int id)
        {//todo:test
            return JsonConvert.SerializeObject(_repository.GetFullInstanceById(id));
        }

        public void DeleteInstance(int instanceId)
        {//todo:test
            _repository.DeleteInstance(instanceId);
            UpdateTaskList();
            _monik.ApplicationInfo($"Удалена запись {instanceId}");
        }

        public string GetAllSchedulesJson()
        {//todo:test
            return JsonConvert.SerializeObject(_repository.GetAllSchedules()); 
        }

        public string GetAllRecepientGroupsJson()
        {//todo:test
            return JsonConvert.SerializeObject(_repository.GetAllRecepientGroups());
        }

        public string GetCurrentViewByTaskId(int taskId)
        {//todo:test
            List<RTask> tasks;
            lock (this)
                tasks = _tasks.ToList();

            var task = tasks.FirstOrDefault(t => t.Id == taskId);

            if (task == null) return "No tasks with such Id found..";
            return task.GetCurrentView();
        }

    } //class
}
