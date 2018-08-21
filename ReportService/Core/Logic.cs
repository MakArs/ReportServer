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
using Autofac.Core;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;

namespace ReportService.Core
{
    public class Logic : ILogic
    {
        private readonly ILifetimeScope autofac;
        private readonly IMapper mapper;
        private readonly IClientControl monik;
        private readonly IArchiver archiver;
        private readonly ITelegramBotClient bot;
        private readonly IRepository repository;
        private readonly Scheduler checkScheduleAndExecuteScheduler;
        private readonly IViewExecutor tableView;

        private readonly List<RRecepientGroup> recepientGroups;
        private readonly List<DtoReport> reports;
        private readonly List<DtoSchedule> schedules;
        private readonly ConcurrentDictionary<long, DtoTelegramChannel> telegramChannels;
        private readonly List<IRTask> tasks;
        private string customViewExecutors;
        private string customDataExecutors;

        public Logic(ILifetimeScope autofac, IRepository repository, IClientControl monik,
                     IMapper mapper, IArchiver archiver, ITelegramBotClient bot)
        {
            this.autofac = autofac;
            this.mapper = mapper;
            this.monik = monik;
            this.archiver = archiver;
            this.bot = bot;
            this.repository = repository;

            checkScheduleAndExecuteScheduler =
                new Scheduler {Period = 60, TaskMethod = CheckScheduleAndExecute};
            tableView = this.autofac.ResolveNamed<IViewExecutor>("tasklistviewex");

            recepientGroups = new List<RRecepientGroup>();
            schedules = new List<DtoSchedule>();
            reports = new List<DtoReport>();
            telegramChannels = new ConcurrentDictionary<long, DtoTelegramChannel>();
            tasks = new List<IRTask>();
            this.bot.OnUpdate += OnBotUpd;

        } //ctor

        private void UpdateRecepientGroupsList()
        {
            var recepList = repository.GetAllRecepientGroups();
            lock (this)
            {
                recepientGroups.Clear();
                foreach (var sched in recepList)
                {
                    recepientGroups.Add(mapper.Map<RRecepientGroup>(sched));
                }
            }
        }

        private void UpdateScheduleList()
        {
            var schedList = repository.GetAllSchedules();
            lock (this)
            {
                schedules.Clear();
                foreach (var sched in schedList)
                    schedules.Add(sched);
            }
        }

        private void UpdateReportsList()
        {
            var repList = repository.GetAllReports();
            lock (this)
            {
                reports.Clear();
                foreach (var rep in repList)
                    reports.Add(rep);
            }
        }

        private void UpdateTelegramChannelsList()
        {
            var chanList = repository.GetAllTelegramChannels();
            lock (this)
            {
                telegramChannels.Clear();
                foreach (var channel in chanList)
                {
                    telegramChannels.TryAdd(channel.ChatId, channel);
                }
            }
        }

        private void UpdateTaskList()
        {
            var taskLst = repository.GetAllTasks();
            lock (this)
            {
                tasks.Clear();

                foreach (var dtoTask in taskLst)
                {
                    var report = reports.First(rep => rep.Id == dtoTask.ReportId);
                    var task = autofac.Resolve<IRTask>(
                        new NamedParameter("id", dtoTask.Id),
                        new NamedParameter("reportName", report.Name),
                        new NamedParameter("template", report.ViewTemplate),
                        new NamedParameter("schedule", schedules
                            .FirstOrDefault(s => s.Id == dtoTask.ScheduleId)),
                        new NamedParameter("query", report.Query),
                        new NamedParameter("telegramChannel", telegramChannels
                            .FirstOrDefault(tc => tc.Value.Id == dtoTask.TelegramChannelId).Value),
                        new NamedParameter("sendAddress", recepientGroups
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

                    tasks.Add(task);
                }
            } //lock
        }

        private void CheckScheduleAndExecute()
        {
            List<IRTask> currentTasks;
            lock (this)
                currentTasks = tasks.ToList();

            CultureInfo.CurrentCulture = new CultureInfo("en-US");

            foreach (var task in currentTasks.Where(x => x.Schedule != null))
            {
                string[] cronStrings =
                    schedules.First(s => s.Id == task.Schedule.Id).Schedule.Split(';');

                foreach (var cronString in cronStrings)
                {
                    var cronSchedule = CrontabSchedule.TryParse(cronString);

                  if (cronSchedule == null) continue;

                    var occurrences =
                        cronSchedule.GetNextOccurrences(task.LastTime, DateTime.Now);
                    if (!occurrences.Any()) continue;

                    ExecuteTask(task);
                    break;
                }
            } //for
        }

        private void ExecuteTask(IRTask task)
        {
            task.UpdateLastTime();
            monik.ApplicationInfo($"Отсылка отчёта {task.Id} по расписанию");
            Task.Factory.StartNew(() => task.Execute());
        }

        private void CreateBase(string connStr)
        {
            try
            {
                repository.CreateBase(connStr);
            }
            catch (Exception e)
            {
                monik.ApplicationError(e.Message);
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
            customDataExecutors = JsonConvert
                .SerializeObject(autofac
                    .ComponentRegistry
                    .Registrations
                    .Where(r => typeof(IDataExecutor)
                        .IsAssignableFrom(r.Activator.LimitType))
                    .Select(r => (r.Services.ToList().First() as KeyedService)?
                        .ServiceKey.ToString())
                    .Where(key => key != "commondataex")
                    .ToList());

            customViewExecutors = JsonConvert
                .SerializeObject(autofac.ComponentRegistry.Registrations
                    .Where(r => typeof(IViewExecutor)
                        .IsAssignableFrom(r.Activator.LimitType))
                    .Select(r => (r.Services.ToList().First() as KeyedService)?
                        .ServiceKey.ToString())
                    .Where(key => key != "commonviewex")
                    .ToList());
            UpdateScheduleList();
            UpdateRecepientGroupsList();
            UpdateReportsList();
            UpdateTelegramChannelsList();
            UpdateTaskList();
            bot.StartReceiving();
            checkScheduleAndExecuteScheduler.OnStart();
        }

        public void Stop()
        {
            checkScheduleAndExecuteScheduler.OnStop();
        }

        public string ForceExecute(int taskId, string mail)
        {
            List<IRTask> currentTasks;

            lock (this)
                currentTasks = tasks.ToList();

            var task = currentTasks.FirstOrDefault(t => t.Id == taskId);

            if (task == null) return "No tasks with such Id found..";
            monik.ApplicationInfo($"Отсылка отчёта {task.Id} на адрес {mail} (ручной запуск)");

            Task.Factory.StartNew(() => task.Execute(mail));
            return $"Report {taskId} sent!";
        }

        public string GetTaskList_HtmlPage()
        {
            List<IRTask> currentTasks;
            lock (this)
                currentTasks = tasks.ToList();

            var tasksView = currentTasks.Select(t => new
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
            var tr = tableView.ExecuteHtml("", jsonTasks);
            return tr;
        }

        public string GetFullInstanceList_HtmlPage(int taskId)
        {
            List<DtoFullInstance> instancesByteData = repository.GetFullInstancesByTaskId(taskId);
            var instances = new List<RFullInstance>();
            foreach (var instance in instancesByteData)
            {
                var rinstance = mapper.Map<RFullInstance>(instance);
                rinstance.Data = archiver.ExtractFromByteArchive(instance.Data);
                rinstance.ViewData = archiver.ExtractFromByteArchive(instance.ViewData);
                instances.Add(rinstance);
            }

            var jsonInstances = JsonConvert.SerializeObject(instances);
            return tableView.ExecuteHtml("", jsonInstances);
        }

        public string GetAllTasksJson()
        {
            List<IRTask> currentTasks;
            lock (this)
                currentTasks = tasks.ToList();
            var tr = JsonConvert.SerializeObject(currentTasks
                .Select(t => mapper.Map<ApiTask>(t)));
            return tr;
        }

        public string GetFullTaskByIdJson(int id)
        {
            List<IRTask> currentTasks;
            lock (this)
                currentTasks = tasks.ToList();
            return JsonConvert.SerializeObject(
                mapper.Map<ApiFullTask>(currentTasks.First(t => t.Id == id)));
        }

        public void DeleteTask(int taskId)
        {
            repository.DeleteEntity<DtoTask>(taskId);
            UpdateTaskList();
            monik.ApplicationInfo($"Удалена задача {taskId}");
        }

        public int CreateTask(ApiTask task)
        {
            var dtoTask = mapper.Map<DtoTask>(task);
            var newTaskId = repository.CreateEntity(dtoTask);
            UpdateTaskList();
            monik.ApplicationInfo($"Создана задача {newTaskId}");
            return newTaskId;
        }

        public void UpdateTask(ApiTask task)
        {
            var dtoTask = mapper.Map<DtoTask>(task);
            repository.UpdateEntity(dtoTask);
            UpdateTaskList();
            monik.ApplicationInfo($"Обновлена задача {task.Id}");
        }

        public string GetAllInstancesJson()
        {
            return JsonConvert.SerializeObject(repository.GetAllInstances());
        }

        public string GetAllInstancesByTaskIdJson(int taskId)
        {
            return JsonConvert.SerializeObject(repository.GetInstancesByTaskId(taskId));
        }

        public string GetFullInstanceByIdJson(int id)
        {
            var instance = repository.GetFullInstanceById(id);
            var rinstance = mapper.Map<RFullInstance>(instance);
            rinstance.Data = archiver.ExtractFromByteArchive(instance.Data);
            rinstance.ViewData = archiver.ExtractFromByteArchive(instance.ViewData);
            return JsonConvert.SerializeObject(rinstance);
        }

        public void DeleteInstance(int instanceId)
        {
            repository.DeleteEntity<DtoInstance>(instanceId);
            UpdateTaskList();
            monik.ApplicationInfo($"Удалена запись {instanceId}");
        }

        public int CreateReport(DtoReport report)
        {
            var reportId = repository.CreateEntity(report);
            UpdateReportsList();
            monik.ApplicationInfo($"Добавлен отчёт {reportId}");
            return reportId;
        }

        public void UpdateReport(DtoReport report)
        {
            repository.UpdateEntity(report);
            UpdateReportsList();
            UpdateTaskList();
            monik.ApplicationInfo($"Обновлён отчёт {report.Id}");
        }

        public string GetAllSchedulesJson()
        {
            return JsonConvert.SerializeObject(schedules);
        }

        public string GetAllRecepientGroupsJson()
        {
            return JsonConvert.SerializeObject(repository.GetAllRecepientGroups());
        }

        public string GetAllReportsJson()
        {
            return JsonConvert.SerializeObject(reports);
        }

        public string GetCurrentViewByTaskId(int taskId)
        {
            List<IRTask> currentTasks;
            lock (this)
                currentTasks = tasks.ToList();

            var task = currentTasks.FirstOrDefault(t => t.Id == taskId);

            if (task == null) return "No tasks with such Id found..";
            return task.GetCurrentView();
        }

        public string GetAllCustomDataExecutors()
        {
            return customDataExecutors;
        }

        public string GetAllCustomViewExecutors()
        {
            return customViewExecutors;
        }

        private void OnBotUpd(object sender, Telegram.Bot.Args.UpdateEventArgs e)
        {
            long chatId = 0;
            string chatName = "";
            ChatType chatType = ChatType.Private;
            UpdateType updType = e.Update.Type;
            switch (updType)
            {
                case UpdateType.ChannelPost:
                    chatId = e.Update.ChannelPost.Chat.Id;
                    chatName = e.Update.ChannelPost.Chat.Title;
                    chatType = ChatType.Channel;
                    break;
                case UpdateType.Message:
                    chatType = e.Update.Message.Chat.Type;
                    chatId = e.Update.Message.Chat.Id;
                    switch (chatType)
                    {
                        case ChatType.Private:
                            chatName =
                                $"{e.Update.Message.Chat.FirstName} {e.Update.Message.Chat.LastName}";
                            break;

                        case ChatType.Group:
                            chatName = e.Update.Message.Chat.Title;
                            break;
                    }

                    break;
            }

            if (chatId != 0 && !telegramChannels.ContainsKey(chatId))
            {
                DtoTelegramChannel channel =
                    new DtoTelegramChannel
                    {
                        ChatId = chatId,
                        Name = string.IsNullOrEmpty(chatName) ? "NoName" : chatName,
                        Type = (int) chatType
                    };

                channel.Id = repository.CreateEntity(channel);

                lock (this)
                {
                    telegramChannels.TryAdd(channel.ChatId, channel);
                }
            }
        }

    } //class
}