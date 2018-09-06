using Autofac;
using AutoMapper;
using Monik.Client;
using NCrontab;
using Newtonsoft.Json;
using ReportService.Interfaces;
using ReportService.Nancy;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Autofac.Core;
using Gerakul.FastSql;
using ReportService.DataExporters;
using ReportService.DataImporters;
using ReportService.Extensions;
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

        private readonly List<DtoOper> operations;
        private readonly List<DtoRecepientGroup> recepientGroups;
        private readonly List<DtoTelegramChannel> telegramChannels;
        private readonly List<DtoSchedule> schedules;
        private readonly List<IRTask> tasks;
        private readonly List<DtoTaskOper> taskOpers;
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

            operations = new List<DtoOper>();
            recepientGroups = new List<DtoRecepientGroup>();
            telegramChannels = new List<DtoTelegramChannel>();
            schedules = new List<DtoSchedule>();
            tasks = new List<IRTask>();
            taskOpers = new List<DtoTaskOper>();

            this.bot.OnUpdate += OnBotUpd;
        } //ctor

        private void UpdateDtoEntitiesList<T>(List<T> list) where T : IDtoEntity, new()
        {
            var repositoryList = repository.GetListEntitiesByDtoType<T>();
            if (repositoryList == null) return;
            lock (this)
            {
                list.Clear();
                foreach (var entity in repositoryList)
                    list.Add(entity);
            }
        }

        private void UpdateTaskList()
        {
            var taskList = repository.GetListEntitiesByDtoType<DtoTask>();
            if (taskList == null) return;
            lock (this)
            {
                tasks.Clear();

                foreach (var dtoTask in taskList)
                {
                    var task = autofac.Resolve<IRTask>(
                        new NamedParameter("id", dtoTask.Id),
                        new NamedParameter("name", dtoTask.Name),
                        new NamedParameter("schedule", schedules
                            .FirstOrDefault(s => s.Id == dtoTask.ScheduleId)),
                        new NamedParameter("operations", operations
                            .Where(oper =>
                                taskOpers
                                    .Where(taskOper => taskOper.TaskId == dtoTask.Id)
                                    .Select(taskOper => taskOper.OperId)
                                    .Contains(oper.Id))
                            .ToList()));

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
            }
        }

        private void ExecuteTask(IRTask task)
        {
            task.UpdateLastTime();
            monik.ApplicationInfo($"Отсылка отчёта {task.Id} по расписанию");
            Task.Factory.StartNew(() => task.Execute());
        }

        //private void CreateBase(string connStr)
        //{
        //        repository.CreateBase(connStr);
        //}

        public void Start()
        {
            
            customDataExecutors = JsonConvert
                .SerializeObject(autofac
                    .ComponentRegistry
                    .Registrations
                    .Where(r => typeof(IDataImporter)
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

            UpdateDtoEntitiesList(operations);
            UpdateDtoEntitiesList(recepientGroups);
            UpdateDtoEntitiesList(telegramChannels);
            UpdateDtoEntitiesList(schedules);
            UpdateDtoEntitiesList(taskOpers);

            UpdateTaskList();

            checkScheduleAndExecuteScheduler.OnStart();
        }

        public void Stop()
        {
            checkScheduleAndExecuteScheduler.OnStop();
        }

        public string ForceExecute(int taskId, string mail) //todo: remake method with new DB conception
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

        #region getListJson

        public string GetAllOperationsJson()
        {
            return JsonConvert.SerializeObject(operations);
        }

        public string GetAllRecepientGroupsJson()
        {
            return JsonConvert.SerializeObject(recepientGroups);
        }

        public string GetAllTelegramChannelsJson()
        {
            return JsonConvert.SerializeObject(telegramChannels);
        }

        public string GetAllSchedulesJson()
        {
            return JsonConvert.SerializeObject(schedules);
        }

        public string GetAllTaskOpersJson()
        {
            return JsonConvert.SerializeObject(taskOpers);
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

        //public string GetEntitiesListJsonByType<T>()
        //{
        //    var list = GetType().GetFields(
        //            BindingFlags.NonPublic |
        //            BindingFlags.Instance)
        //        .FirstOrDefault(field => field.FieldType == typeof(List<T>))?
        //        .GetValue(this);
        //    return JsonConvert.SerializeObject(list);
        //}

        #endregion

        public int CreateOperation(DtoOper oper)
        {
            var newExporterId = repository.CreateEntity(oper);
            UpdateDtoEntitiesList(operations);
            monik.ApplicationInfo($"Создана настройка операции {newExporterId}");
            return newExporterId;
        }

        public void UpdateOperation(DtoOper oper)
        {
            repository.UpdateEntity(oper);
            UpdateDtoEntitiesList(operations);
            UpdateTaskList();
            monik.ApplicationInfo($"Обновлена настройка операции {oper.Id}");
        }

        public int CreateRecepientGroup(DtoRecepientGroup group)
        {
            var newGroupId = repository.CreateEntity(group);
            UpdateDtoEntitiesList(recepientGroups);
            monik.ApplicationInfo($"Создана группа получателей {newGroupId}");
            return newGroupId;
        }

        public void UpdateRecepientGroup(DtoRecepientGroup group)
        {
            repository.UpdateEntity(group);
            UpdateDtoEntitiesList(recepientGroups);
            UpdateTaskList();
            monik.ApplicationInfo($"Обновлена группа получателей {group.Id}");
        }

        public RecepientAddresses GetRecepientAddressesByGroupId(int groupId)
        {
            return mapper.Map<RRecepientGroup>(recepientGroups
                .FirstOrDefault(group => group.Id == groupId)).GetAddresses();
        }

        public int CreateTelegramChannel(DtoTelegramChannel channel)
        {
            var newChannelId = repository.CreateEntity(channel);
            UpdateDtoEntitiesList(recepientGroups);
            monik.ApplicationInfo($"Добавлен телеграм канал {newChannelId}");
            return newChannelId;
        }

        public void UpdateTelegramChannel(DtoTelegramChannel channel)
        {
            repository.UpdateEntity(channel);
            UpdateDtoEntitiesList(recepientGroups);
            UpdateTaskList();
            monik.ApplicationInfo($"Обновлена телеграм канал  {channel.Id}");
        }

        public DtoTelegramChannel GetTelegramChatIdByChannelId(int id)
        {
            return telegramChannels
                .FirstOrDefault(channel => channel.Id == id);
        }

        public int CreateSchedule(DtoSchedule schedule)
        {
            var newScheduleId = repository.CreateEntity(schedule);
            UpdateDtoEntitiesList(schedules);
            monik.ApplicationInfo($"Создано расписание {newScheduleId}");
            return newScheduleId;
        }

        public void UpdateSchedule(DtoSchedule schedule)
        {
            repository.UpdateEntity(schedule);
            UpdateDtoEntitiesList(schedules);
            monik.ApplicationInfo($"Обновлено расписание {schedule.Id}");
        }

        public int CreateTaskOper(DtoTaskOper taskOper)
        {
            var newtaskOperId = repository.CreateEntity(taskOper);
            UpdateDtoEntitiesList(operations);
            UpdateTaskList();
            monik.ApplicationInfo($"В задачу {taskOper.TaskId} добавлен экспортёр {taskOper.OperId}");
            return newtaskOperId;
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

        public void DeleteTask(int taskId)
        {
            repository.DeleteEntity<DtoTask>(taskId);
            UpdateTaskList();
            monik.ApplicationInfo($"Удалена задача {taskId}");
        }

        public string GetTaskList_HtmlPage() //todo: remake method with new DB conception
        {
            List<IRTask> currentTasks;
            lock (this)
                currentTasks = tasks.ToList();

            var tasksView = currentTasks.Select(t => new
                {
                    t.Id,
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

        public string GetCurrentViewByTaskId(int taskId)
        {
            List<IRTask> currentTasks;
            lock (this)
                currentTasks = tasks.ToList();

            var task = currentTasks.FirstOrDefault(t => t.Id == taskId);

            if (task == null) return "No tasks with such Id found..";
            return task.GetCurrentView();
        }

        public void DeleteTaskInstanceById(int id)
        {
            repository.DeleteEntity<DtoTaskInstance>(id);
            UpdateTaskList();
            monik.ApplicationInfo($"Удалена запись {id}");
        }

        public string GetAllTaskInstancesJson()
        {
            return JsonConvert.SerializeObject(
                repository.GetListEntitiesByDtoType<DtoTaskInstance>());
        }

        public string GetAllTaskInstancesByTaskIdJson(int taskId)
        {
            return JsonConvert.SerializeObject(repository.GetInstancesByTaskId(taskId));
        }

        public string GetFullInstanceList_HtmlPage(int taskId) //todo: remake method with new DB conception
        {
            DtoOperInstance instancesByteData = repository.GetFullOperInstanceById(taskId);
            var instances = new List<RFullInstance>();

            //foreach (var instance in instancesByteData)
            //{
            //    var rinstance = mapper.Map<RFullInstance>(instance);
            //    rinstance.Data = archiver.ExtractFromByteArchive(instance.Data);
            //    rinstance.ViewData = archiver.ExtractFromByteArchive(instance.ViewData);
            //    instances.Add(rinstance);
            //}

            var jsonInstances = JsonConvert.SerializeObject(instances);
            return tableView.ExecuteHtml("", jsonInstances);
        }

        public void DeleteOperInstanceById(int operInstanceId)
        {
            repository.DeleteEntity<DtoOperInstance>(operInstanceId);
        }

        public string GetAllOperInstancesByTaskInstanceIdJson(int taskInstanceId)
        {
            return JsonConvert.SerializeObject(repository
                .GetOperInstancesByTaskInstanceId(taskInstanceId));
        }

        public string GetFullOperInstanceByIdJson(int id)
        {
            var instance = repository.GetFullOperInstanceById(id);
            var rinstance = mapper.Map<RFullInstance>(instance);
            rinstance.Data = archiver.ExtractFromByteArchive(instance.DataSet);
            return JsonConvert.SerializeObject(rinstance);
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

            if (chatId != 0 && !telegramChannels.Select(channel => channel.ChatId).Contains(chatId))
            {
                DtoTelegramChannel channel =
                    new DtoTelegramChannel
                    {
                        ChatId = chatId,
                        Name = string.IsNullOrEmpty(chatName) ? "NoName" : chatName,
                        Type = (int) chatType
                    };

                channel.Id = repository.CreateEntity(channel);
                UpdateDtoEntitiesList(telegramChannels);
            }
        }

    } //class
}
