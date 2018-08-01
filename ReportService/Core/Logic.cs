using Autofac;
using AutoMapper;
using Monik.Client;
using NCrontab;
using Newtonsoft.Json;
using ReportService.Interfaces;
using ReportService.Nancy;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;

namespace ReportService.Core
{
    public class Logic : ILogic
    {
        private readonly ILifetimeScope _autofac;
        private readonly IMapper _mapper;
        private readonly IClientControl _monik;
        private readonly IArchiver _archiver;
        private readonly ITelegramBotClient _bot;
        private readonly IRepository _repository;
        private readonly Scheduler _checkScheduleAndExecuteScheduler;
        private readonly IViewExecutor _tableView;

        private readonly List<RRecepientGroup> _recepientGroups;
        private readonly List<DtoSchedule> _schedules;
        private readonly List<DtoReport> _reports;
        private readonly ConcurrentDictionary<long, DtoTelegramChannel> _telegramChannels;
        private readonly List<IRTask> _tasks;

        public Logic(ILifetimeScope autofac, IRepository repository, IClientControl monik,
                     IMapper mapper, IArchiver archiver, ITelegramBotClient bot)
        {
            _autofac    = autofac;
            _mapper     = mapper;
            _monik      = monik;
            _archiver   = archiver;
            _bot        = bot;
            _repository = repository;

            _checkScheduleAndExecuteScheduler = new Scheduler {Period = 60, TaskMethod = CheckScheduleAndExecute};
            _tableView                        = _autofac.ResolveNamed<IViewExecutor>("tasklistviewex");

            _recepientGroups  =  new List<RRecepientGroup>();
            _schedules        =  new List<DtoSchedule>();
            _reports          =  new List<DtoReport>();
            _telegramChannels =  new ConcurrentDictionary<long, DtoTelegramChannel>();
            _tasks            =  new List<IRTask>();
            _bot.OnUpdate     += OnBotUpd;

        } //ctor

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

        private void UpdateTelegramChannelsList()
        {
            var chanList = _repository.GetAllTelegramChannels();
            lock (this)
            {
                _telegramChannels.Clear();
                foreach (var channel in chanList)
                {
                    _telegramChannels.TryAdd(channel.ChatId, channel);
                }
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
                        new NamedParameter("reportName", report.Name),
                        new NamedParameter("template", report.ViewTemplate),
                        new NamedParameter("schedule", _schedules
                            .FirstOrDefault(s => s.Id == dtoTask.ScheduleId)),
                        new NamedParameter("query", report.Query),
                        new NamedParameter("chatId", _telegramChannels
                            .FirstOrDefault(tc => tc.Value.Id == dtoTask.TelegramChannelId).Value?.ChatId),
                        new NamedParameter("sendAddress", _recepientGroups
                            .FirstOrDefault(r => r.Id == dtoTask.RecepientGroupId)),
                        new NamedParameter("tryCount", dtoTask.TryCount),
                        new NamedParameter("timeOut", report.QueryTimeOut),
                        new NamedParameter("reportType", (RReportType) report.ReportType),
                        new NamedParameter("connStr", report.ConnectionString),
                        new NamedParameter("reportId", report.Id),
                        new NamedParameter("htmlBody", dtoTask.HasHtmlBody),
                        new NamedParameter("jsonAttach", dtoTask.HasJsonAttachment),
                        new NamedParameter("xlsxAttach", dtoTask.HasXlsxAttachment)
                    );

                    // might be replaced with saved time from db
                    task.UpdateLastTime();

                    _tasks.Add(task);
                }
            } //lock
        }

        private void CheckScheduleAndExecute()
        {
            List<IRTask> tasks;
            lock (this)
                tasks = _tasks.ToList();

            DateTime time = DateTime.Now;
            CultureInfo.CurrentCulture = new CultureInfo("en-US");

            foreach (var task in tasks.Where(x => x.Schedule != null))
            {
                string[] cronStrings = _schedules.First(s => s.Id == task.Schedule.Id).Schedule.Split(';');

                foreach (var cronString in cronStrings)
                {
                    var cronSchedule = CrontabSchedule.TryParse(cronString);
                    if (cronSchedule != null)
                    {
                        var occurrences = cronSchedule.GetNextOccurrences(task.LastTime, DateTime.Now);
                        if (occurrences.Any())
                        {
                            ExecuteTask(task);
                            break;
                        }
                    }
                }
            } //for
        }

        private void ExecuteTask(IRTask task)
        {
            task.UpdateLastTime();
            _monik.ApplicationInfo($"Отсылка отчёта {task.Id} по расписанию");
            Task.Factory.StartNew(() => task.Execute());
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
            //try
            //{
            //CreateBase(ConfigurationManager.AppSettings["DBConnStr"]);
            //}
            //catch (Exception e)
            //{
            //    Console.WriteLine(e);
            //}
            UpdateScheduleList();
            UpdateRecepientGroupsList();
            UpdateReportsList();
            UpdateTelegramChannelsList();
            UpdateTaskList();
            _bot.StartReceiving();
            _checkScheduleAndExecuteScheduler.OnStart();
        }

        public void Stop()
        {
            _checkScheduleAndExecuteScheduler.OnStop();
        }

        public string ForceExecute(int taskId, string mail)
        {
            List<IRTask> tasks;

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
            List<IRTask> tasks;
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
            var tr        = _tableView.ExecuteHtml("", jsonTasks);
            return tr;
        }

        public string GetFullInstanceList_HtmlPage(int taskId)
        {
            List<DtoFullInstance> instancesByteData = _repository.GetFullInstancesByTaskId(taskId);
            var                   instances         = new List<RFullInstance>();
            foreach (var instance in instancesByteData)
            {
                var rinstance = _mapper.Map<RFullInstance>(instance);
                rinstance.Data     = _archiver.ExtractFromByteArchive(instance.Data);
                rinstance.ViewData = _archiver.ExtractFromByteArchive(instance.ViewData);
                instances.Add(rinstance);
            }

            var jsonInstances = JsonConvert.SerializeObject(instances);
            return _tableView.ExecuteHtml("", jsonInstances);
        }

        public string GetAllTasksJson()
        {
            List<IRTask> tasks;
            lock (this)
                tasks = _tasks.ToList();
            var tr = JsonConvert.SerializeObject(tasks
                .Select(t => _mapper.Map<ApiTask>(t)));
            return tr;
        }

        public string GetFullTaskByIdJson(int id)
        {
            List<IRTask> tasks;
            lock (this)
                tasks = _tasks.ToList();
            return JsonConvert.SerializeObject(_mapper.Map<ApiFullTask>(tasks.First(t => t.Id == id)));
        }

        public void DeleteTask(int taskId)
        {
            _repository.DeleteEntity<DtoTask>(taskId);
            UpdateTaskList();
            _monik.ApplicationInfo($"Удалена задача {taskId}");
        }

        public int CreateTask(ApiFullTask task)
        {
            var dtoTask   = _mapper.Map<DtoTask>(task);
            var newTaskId = _repository.CreateEntity(dtoTask);
            UpdateTaskList();
            _monik.ApplicationInfo($"Создана задача {newTaskId}");
            return newTaskId;
        }

        public void UpdateTask(ApiFullTask task)
        {
            var dtoTask = _mapper.Map<DtoTask>(task);
            _repository.UpdateEntity(dtoTask);
            UpdateTaskList();
            _monik.ApplicationInfo($"Обновлена задача {task.Id}");
        }

        public string GetAllInstancesJson()
        {
            return JsonConvert.SerializeObject(_repository.GetAllInstances());
        }

        public string GetAllInstancesByTaskIdJson(int taskId)
        {
            return JsonConvert.SerializeObject(_repository.GetInstancesByTaskId(taskId));
        }

        public string GetFullInstanceByIdJson(int id)
        {
            var instance  = _repository.GetFullInstanceById(id);
            var rinstance = _mapper.Map<RFullInstance>(instance);
            rinstance.Data     = _archiver.ExtractFromByteArchive(instance.Data);
            rinstance.ViewData = _archiver.ExtractFromByteArchive(instance.ViewData);
            return JsonConvert.SerializeObject(rinstance);
        }

        public void DeleteInstance(int instanceId)
        {
            _repository.DeleteEntity<DtoInstance>(instanceId);
            UpdateTaskList();
            _monik.ApplicationInfo($"Удалена запись {instanceId}");
        }

        public int CreateReport(DtoReport report)
        {
            var reportId = _repository.CreateEntity(report);
            UpdateReportsList();
            _monik.ApplicationInfo($"Добавлен отчёт {reportId}");
            return reportId;
        }

        public void UpdateReport(DtoReport report)
        {
            _repository.UpdateEntity(report);
            UpdateReportsList();
            UpdateTaskList();
            _monik.ApplicationInfo($"Обновлён отчёт {report.Id}");
        }

        public string GetAllSchedulesJson()
        {
            return JsonConvert.SerializeObject(_schedules);
        }

        public string GetAllRecepientGroupsJson()
        {
            return JsonConvert.SerializeObject(_repository.GetAllRecepientGroups());
        }

        public string GetAllReportsJson()
        {
            return JsonConvert.SerializeObject(_reports);
        }

        public string GetCurrentViewByTaskId(int taskId)
        {
            List<IRTask> tasks;
            lock (this)
                tasks = _tasks.ToList();

            var task = tasks.FirstOrDefault(t => t.Id == taskId);

            if (task == null) return "No tasks with such Id found..";
            return task.GetCurrentView();
        }

        private void OnBotUpd(object sender, Telegram.Bot.Args.UpdateEventArgs e)
        {
            long       chatId   = 0;
            string     chatName = "";
            ChatType   chatType = ChatType.Private;
            UpdateType updType  = e.Update.Type;
            switch (updType)
            {
                case UpdateType.ChannelPost:
                    chatId   = e.Update.ChannelPost.Chat.Id;
                    chatName = e.Update.ChannelPost.Chat.Title;
                    chatType = ChatType.Channel;
                    break;
                case UpdateType.Message:
                    chatType = e.Update.Message.Chat.Type;
                    chatId   = e.Update.Message.Chat.Id;
                    switch (chatType)
                    {
                        case ChatType.Private:
                            chatName = $"{e.Update.Message.Chat.FirstName} {e.Update.Message.Chat.LastName}";
                            break;

                        case ChatType.Group:
                            chatName = e.Update.Message.Chat.Title;
                            break;
                    }

                    break;
            }

            if (chatId != 0 && !_telegramChannels.ContainsKey(chatId))
            {
                DtoTelegramChannel channel =
                    new DtoTelegramChannel
                    {
                        ChatId = chatId,
                        Name   = string.IsNullOrEmpty(chatName) ? "NoName" : chatName,
                        Type   = (int) chatType
                    };

                channel.Id = _repository.CreateEntity(channel);

                lock (this)
                {
                    _telegramChannels.TryAdd(channel.ChatId, channel);
                }
            }
        }

    } //class
}