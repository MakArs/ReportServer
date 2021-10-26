using Autofac;
using AutoMapper;
using NCrontab;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Autofac.Core;
using Microsoft.Extensions.Configuration;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;
using Monik.Common;
using ReportService.Entities;
using ReportService.Entities.Dto;
using ReportService.Interfaces.Core;
using ReportService.Interfaces.Operations;
using ReportService.Interfaces.Protobuf;
using ReportService.Interfaces.ReportTask;
using ReportService.Operations.DataExporters.Configurations;

namespace ReportService.Core
{
    public class Logic : ILogic
    {
        private readonly ILifetimeScope autofac;
        private readonly IMapper mapper;
        private readonly IMonik monik;
        private readonly ITelegramBotClient bot;

        private readonly IRepository repository;

        //private readonly Scheduler checkScheduleAndExecuteScheduler;
        private readonly IViewExecutor tableView;
        private readonly IPackageBuilder packageBuilder;

        private readonly List<DtoOperTemplate> operTemplates;
        private readonly List<DtoRecepientGroup> recepientGroups;
        private readonly List<DtoTelegramChannel> telegramChannels;
        private readonly List<DtoSchedule> schedules;
        private readonly List<IReportTask> tasks;
        private readonly List<DtoOperation> operations;
        private readonly Dictionary<long, IReportTaskRunContext> contextsInWork;
        private readonly List<TaskRequestInfo> taskRequestInfos;

        public Dictionary<string, Type> RegisteredExporters { get; set; }
        public Dictionary<string, Type> RegisteredImporters { get; set; }

        public Logic(ILifetimeScope autofac, IRepository repository, IMonik monik,
            IMapper mapper, ITelegramBotClient bot, IPackageBuilder builder)
        {
            this.autofac = autofac;
            this.mapper = mapper;
            this.monik = monik;
            this.bot = bot;
            bot.StartReceiving();
            this.repository = repository;
            packageBuilder = builder;
            contextsInWork = new Dictionary<long, IReportTaskRunContext>();

            tableView = this.autofac.ResolveNamed<IViewExecutor>("CommonTableViewEx");

            operTemplates = new List<DtoOperTemplate>();
            recepientGroups = new List<DtoRecepientGroup>();
            telegramChannels = new List<DtoTelegramChannel>();
            schedules = new List<DtoSchedule>();
            tasks = new List<IReportTask>();
            operations = new List<DtoOperation>();
            taskRequestInfos = new List<TaskRequestInfo>();

            this.bot.OnUpdate += OnBotUpd;
        } //ctor

        private void UpdateDtoEntitiesList<T>(List<T> list) where T : class, IDtoEntity
        {
            var repositoryList = repository.GetListEntitiesByDtoType<T>();
            if (repositoryList == null) return;

            lock (list)
            {
                list.Clear();
                foreach (var entity in repositoryList)
                    list.Add(entity);
            }
        }

        private void UpdateTaskList()
        {
            try
            {
                var taskList = repository.GetListEntitiesByDtoType<DtoTask>();
                if (taskList == null) return;
                lock (tasks)
                {
                    tasks.Clear();

                    foreach (var dtoTask in taskList)
                    {
                        var task = autofac.Resolve<IReportTask>(
                            new NamedParameter("id", dtoTask.Id),
                            new NamedParameter("name", dtoTask.Name),
                            new NamedParameter("parameters", dtoTask.Parameters),
                            new NamedParameter("dependsOn", dtoTask.DependsOn),
                            new NamedParameter("schedule", schedules
                                .FirstOrDefault(s => s.Id == dtoTask.ScheduleId)),
                            new NamedParameter("opers", operations
                                .Where(oper => oper.TaskId == dtoTask.Id)
                                .Where(oper => !oper.IsDeleted).ToList()),
                        new NamedParameter("parameterInfos", dtoTask.ParameterInfos));

                        //todo: might be replaced with saved time from db
                        task.UpdateLastTime();
                        tasks.Add(task);
                    }
                } //lock
            }

            catch (Exception e)
            {
                var msg = $"Error while updating tasks: {e.Message}";
                monik.ApplicationError(msg);
                Console.WriteLine(msg);
            }
        } //taskresolver

        public void CheckScheduleAndExecute()
        {
            monik.ApplicationInfo($"{DateTime.Now}");

            List<IReportTask> currentTasks;
            lock (tasks)
                currentTasks = tasks.ToList();

            CultureInfo.CurrentCulture = new CultureInfo("en-US");

            foreach (var task in currentTasks.Where(x => x.Schedule != null))
            {
                try
                {
                    if (isTaskNeedToRun(task))
                    {
                        ExecuteTask(task);
                    }
                }
                catch (Exception ex)
                {
                    monik.ApplicationError($"CheckScheduleAndExecute error in task {task.Id} with schedule {task.Schedule?.Schedule}: {ex}");
                }
            }

            List<TaskRequestInfo> currentTasksInfos;
            currentTasksInfos = taskRequestInfos.ToList();

            foreach (var taskInfo in currentTasksInfos)
            {
                if (taskInfo.Status == (int)RequestStatus.Pending)
                {
                    var task = currentTasks.FirstOrDefault(x => x.Id == taskInfo.TaskId);

                    try
                    {
                        var newTaskParams = JsonConvert.DeserializeObject<List<TaskParameter>>(taskInfo.Parameters).ToDictionary(x => x.Name, x => x.Value);
                        RequestExecuteTask(task, newTaskParams, taskInfo);
                    }
                    catch (Exception ex)
                    {
                        taskInfo.Status = (int)RequestStatus.Failed;
                        repository.UpdateEntity(taskInfo);
                        monik.ApplicationError($"RequestExecute error in taskRequest {taskInfo.RequestId} with error: {ex}");
                    }
                }
            }
        }

        private void RequestExecuteTask(IReportTask task, Dictionary<string, string> parameters, TaskRequestInfo taskRequestInfo)
        {
            SendServiceInfo($"Executing task {task.Id} (request)");

            var context = task.GetCurrentContext(false);

            if (context is null)
                return;

            foreach (var item in parameters)
            {
                context.Parameters[item.Key] = item.Value;
            }

            context.TaskRequestInfo = taskRequestInfo;

            var instanceId = context.TaskInstance.Id;
            contextsInWork.Add(instanceId, context);

            Task.Factory.StartNew(() => task.Execute(context), context.CancelSource.Token)
                .ContinueWith(_ => EndContextWork(instanceId));

            task.UpdateLastTime();
        }

        private void ExecuteTask(IReportTask task)
        {
            SendServiceInfo($"Executing task {task.Id} (scheduled)");

            var context = task.GetCurrentContext(false);

            if (context == null)
                return;

            var instanceId = context.TaskInstance.Id;

            contextsInWork.Add(instanceId, context);

            Task.Factory.StartNew(() => task.Execute(context), context.CancelSource.Token)
                .ContinueWith(_ => EndContextWork(instanceId));

            task.UpdateLastTime();
        }

        private void EndContextWork(long taskInstanceId)
        {
            if (!contextsInWork.ContainsKey(taskInstanceId))
                return;

            var context = contextsInWork[taskInstanceId];

            context.CancelSource.Dispose();
            contextsInWork.Remove(taskInstanceId);
        }

        public void CancelContextWork(long taskInstanceId)
        {
            var context = contextsInWork[taskInstanceId];
            context.CancelSource.Cancel();

            context.TaskInstance.State = (int)InstanceState.Canceled;
            if (context.TaskRequestInfo != null)
                context.TaskRequestInfo.Status = (int)RequestStatus.Canceled;
            repository.UpdateEntity(context.TaskInstance);
            contextsInWork.Remove(taskInstanceId);
        }

        private void CreateBase(string connStr)
        {
            repository.CreateSchema(connStr);
        }

        private Dictionary<string, Type> GetRegistrationsByTypeAndKeyType<T, TU>()
        {
            return autofac //todo:change ugly code (gets autofac registration names)?
                .ComponentRegistry
                .Registrations
                .Where(r => typeof(T)
                    .IsAssignableFrom(r.Activator.LimitType))
                .Where(r =>
                {
                    var serviceKey = ((KeyedService)r.Services.ToList().Last())?.ServiceKey;
                    return serviceKey != null && ((Type)serviceKey).GetInterfaces().Contains(typeof(TU));
                })
                .Select(r =>
                    new KeyValuePair<string, Type>(
                        (r.Services.ToList().First() as KeyedService)?.ServiceKey.ToString(),
                        (r.Services.ToList().Last() as KeyedService)?.ServiceKey as Type)
                ).ToDictionary(pair => pair.Key, pair => pair.Value);
        }

        public void Start()
        {
            //var serviceConfig = autofac.Resolve<IConfigurationRoot>();

            //CreateSchema(serviceConfig["DBConnStr"]);

            RegisteredImporters = GetRegistrationsByTypeAndKeyType<IOperation, IImporterConfig>();
            RegisteredExporters = GetRegistrationsByTypeAndKeyType<IOperation, IExporterConfig>();

            UpdateDtoEntitiesList(operTemplates);
            UpdateDtoEntitiesList(recepientGroups);
            UpdateDtoEntitiesList(telegramChannels);
            UpdateDtoEntitiesList(schedules);
            UpdateDtoEntitiesList(operations);
            UpdateDtoEntitiesList(taskRequestInfos);

            UpdateTaskList();

            UpdateInstances();
        } //start

        private void UpdateInstances()
        {
            var operIds = repository.UpdateOperInstancesAndGetIds();

            if (operIds.Count > 0)
                SendServiceInfo($"Updated unfinished operation instances: {string.Join(",", operIds)}");

            var taskids = repository.UpdateTaskInstancesAndGetIds();

            if (taskids.Count > 0)
                SendServiceInfo($"Updated unfinished task instances: {string.Join(",", taskids)}");
        }

        public string SendDefault(int taskId, string mailAddress)
        {
            List<IReportTask> currentTasks;

            lock (tasks)
                currentTasks = tasks.ToList();

            var task = currentTasks.FirstOrDefault(t => t.Id == taskId);

            if (task == null) return "No tasks with such Id found..";

            SendServiceInfo($"Sending default dataset of task {task.Id} to address" +
                            $" {mailAddress} (launched manually)");

            var context = task.GetCurrentContext(true);

            if (context == null)
                return $"Service is unable to create default context of task {taskId}";

            var instanceId = context.TaskInstance.Id;

            contextsInWork.Add(instanceId, context);

            Task.Factory.StartNew(() => task.SendDefault(context, mailAddress), context.CancelSource.Token)
                .ContinueWith(_ => EndContextWork(instanceId));

            return $"Task {taskId} default dataset sent to {mailAddress}!";
        }

        public string ForceExecute(long taskId)
        {
            List<IReportTask> currentTasks;

            lock (tasks)
                currentTasks = tasks.ToList();

            var task = currentTasks.FirstOrDefault(t => t.Id == taskId);

            if (task == null) return "No tasks with such Id found..";

            SendServiceInfo($"Executing task {task.Id} (launched manually)");

            var context = task.GetCurrentContext(false);

            if (context == null)
                return $"Service is unable to create context of task {taskId}";

            var instanceId = context.TaskInstance.Id;

            contextsInWork.Add(instanceId, context);

            Task.Factory.StartNew(() =>
                     task.Execute(context), context.CancelSource.Token)
                .ContinueWith(_ => EndContextWork(instanceId));
            return $"Task {taskId} executed!";
        }

        #region getListJson

        public string GetAllOperTemplatesJson()
        {
            List<DtoOperTemplate> currentTemplates;
            lock (operTemplates)
                currentTemplates = operTemplates.ToList();

            return JsonConvert.SerializeObject(currentTemplates);
        }

        public string GetAllRecepientGroupsJson()
        {
            List<DtoRecepientGroup> currentRecepients;
            lock (recepientGroups)
                currentRecepients = recepientGroups.ToList();

            return JsonConvert.SerializeObject(currentRecepients);
        }

        public string GetAllTelegramChannelsJson()
        {
            List<DtoTelegramChannel> currentChannels;
            lock (telegramChannels)
                currentChannels = telegramChannels.ToList();

            return JsonConvert.SerializeObject(currentChannels);
        }

        public string GetAllSchedulesJson()
        {
            List<DtoSchedule> currentSchedules;
            lock (schedules)
                currentSchedules = schedules.ToList();

            return JsonConvert.SerializeObject(currentSchedules);
        }

        public string GetAllOperationsJson()
        {
            List<DtoOperation> currentOperations;
            lock (operations)
                currentOperations = operations.Where(oper => !oper.IsDeleted).ToList();

            return JsonConvert.SerializeObject(currentOperations);
        }

        public List<IReportTask> GetAllTasksJson()
        {
            List<IReportTask> currentTasks;
            lock (tasks)
                currentTasks = tasks
                    .ToList();
            return currentTasks;
        }

        public List<TaskRequestInfo> GetListTaskRequestInfoByIds(long[] taskRequestInfoIds)
        {
            return repository.GetListTaskRequestInfoByIds(taskRequestInfoIds);
        }

        public TaskRequestInfo GetTaskRequestInfoById(long id)
        {
            return repository.GetTaskRequestInfoById(id);
        }

        public List<TaskRequestInfo> GetTaskRequestInfoByFilter(RequestStatusFilter requestStatusFilter)
        {
            return repository.GetTaskRequestInfoByFilter(requestStatusFilter);
        }

        public List<TaskRequestInfo> GetTaskRequestInfoByTimePeriod(DateTime timeFrom, DateTime timeTo)
        {
            return repository.GetTaskRequestInfoByTimePeriod(timeFrom, timeTo);
        }

        public List<TaskRequestInfo> GetTaskRequestInfoByTaskIds(long[] taskIds)
        {
            return repository.GetTaskRequestInfoByTaskIds(taskIds);
        }

        public string GetEntitiesCountJson()
        {
            var entities = new Dictionary<string, int>()
            {
                {"operTemplates", operTemplates.Count},
                {"recepientGroups", recepientGroups.Count},
                {"telegramChannels", telegramChannels.Count},
                {"schedules", schedules.Count},
                {"tasks", tasks.Count},
                {"operations", operations.Count}
            };
            return JsonConvert.SerializeObject(entities);
        }

        #endregion

        public long CreateOperationTemplate(DtoOperTemplate operTemplate)
        {
            long newOperId = repository.CreateEntity(operTemplate);
            UpdateDtoEntitiesList(operTemplates);

            SendServiceInfo($"Created operation template {newOperId}");

            return newOperId;
        }

        public void UpdateOperationTemplate(DtoOperTemplate operTemplate)
        {
            repository.UpdateEntity(operTemplate);
            UpdateDtoEntitiesList(operTemplates);
            UpdateTaskList();

            SendServiceInfo($"Changed operation template {operTemplate.Id}");
        }

        public void DeleteOperationTemplate(int id)
        {
            repository.DeleteEntity<DtoOperTemplate, int>(id);
            UpdateDtoEntitiesList(operTemplates);

            SendServiceInfo($"Deleted operation template {id}");
        }

        public long CreateRecepientGroup(DtoRecepientGroup group)
        {
            var newGroupId = repository.CreateEntity(group);
            UpdateDtoEntitiesList(recepientGroups);

            SendServiceInfo($"Created recepient group {newGroupId}");

            return newGroupId;
        }

        public void UpdateRecepientGroup(DtoRecepientGroup group)
        {
            repository.UpdateEntity(group);
            UpdateDtoEntitiesList(recepientGroups);
            UpdateTaskList();
            SendServiceInfo($"Changed recepient group {group.Id}");
        }

        public void DeleteRecepientGroup(int id)
        {
            repository.DeleteEntity<DtoRecepientGroup, int>(id);
            UpdateDtoEntitiesList(recepientGroups);

            SendServiceInfo($"Deleted recepient group {id}");
        }

        public RecipientAddresses GetRecepientAddressesByGroupId(int groupId)
        {
            return mapper.Map<RecipientGroup>(recepientGroups
                .FirstOrDefault(group => group.Id == groupId)).GetAddresses();
        }

        public long CreateTelegramChannel(DtoTelegramChannel channel)
        {
            var newChannelId = repository.CreateEntity(channel);
            UpdateDtoEntitiesList(recepientGroups);
            SendServiceInfo($"Created telegram channel {newChannelId}");
            return newChannelId;
        }

        public void UpdateTelegramChannel(DtoTelegramChannel channel)
        {
            repository.UpdateEntity(channel);
            UpdateDtoEntitiesList(recepientGroups);
            UpdateTaskList();
            SendServiceInfo($"Changed telegram channel  {channel.Id}");
        }

        public DtoTelegramChannel GetTelegramChatIdByChannelId(int id)
        {
            return telegramChannels
                .FirstOrDefault(channel => channel.Id == id);
        }

        public long CreateSchedule(DtoSchedule schedule)
        {
            var newScheduleId = repository.CreateEntity(schedule);
            UpdateDtoEntitiesList(schedules);
            SendServiceInfo($"Created schedule {newScheduleId}");
            return newScheduleId;
        }

        public void UpdateSchedule(DtoSchedule schedule)
        {
            repository.UpdateEntity(schedule);
            UpdateDtoEntitiesList(schedules);

            SendServiceInfo($"Changed schedule {schedule.Id}");
        }

        public void DeleteSchedule(int id)
        {
            repository.DeleteEntity<DtoSchedule, int>(id);
            UpdateDtoEntitiesList(schedules);
            SendServiceInfo($"Deleted schedule {id}");
        }

        public long CreateTask(DtoTask task, DtoOperation[] bindedOpers)
        {
            var newTaskId = repository.CreateTask(task,
                bindedOpers);

            UpdateDtoEntitiesList(operations);
            UpdateTaskList();
            SendServiceInfo($"Created task {newTaskId}");
            return newTaskId;
        }

        public void UpdateTask(DtoTask task, DtoOperation[] bindedOpers)
        {
            repository.UpdateTask(task, bindedOpers);

            UpdateDtoEntitiesList(operations);
            UpdateTaskList();
            SendServiceInfo($"Changed task {task.Id}");
        }

        public void DeleteTask(long taskId)
        {
            repository.DeleteEntity<DtoTask, long>(taskId);
            UpdateDtoEntitiesList(operations);
            UpdateTaskList();
            SendServiceInfo($"Deleted task {taskId}");
        }

        public async Task<string> GetTasksList_HtmlPageAsync()
        {
            List<IReportTask> currentTasks;
            lock (tasks)
                currentTasks = tasks.ToList();

            var tasksData = currentTasks.Select(task => new
            {
                task.Id,
                task.Name,
                task.Schedule?.Schedule,
                Operations = string.Join("=>", task.Operations.Select(oper => oper.Properties.Name))
            });

            var pack = packageBuilder.GetPackage(tasksData);

            return await Task.Factory.StartNew(() =>
                tableView.ExecuteHtml("Current tasks list", pack));
        }

        public string GetWorkingTaskInstancesJson(long taskId)
        {
            return JsonConvert.SerializeObject(contextsInWork.Select(cont => cont.Value)
                .Where(rtask => rtask.TaskId == taskId)
                .Select(rtask => rtask.TaskInstance.Id).ToList());
        }

        public async Task<string> GetTasksInWorkList_HtmlPageAsync()
        {
            List<IReportTaskRunContext> tasksInWork;
            lock (contextsInWork)
                tasksInWork = contextsInWork.Select(pair => pair.Value).ToList();

            var inWorkData = tasksInWork.Select(context => new
            {
                context.TaskId,
                context.TaskName,
                TaskInstanceId = context.TaskInstance.Id,
                TaskStarted = context.TaskInstance.StartTime,
                OperationStates = string.Join(" => ", context.PackageStates)
            });

            var pack = packageBuilder.GetPackage(inWorkData);

            return await Task.Factory.StartNew(() =>
                tableView.ExecuteHtml("Current tasks list", pack));
        }

        public async Task<string> GetCurrentViewAsync(long taskId)
        {
            List<IReportTask> currentTasks;
            lock (tasks)
                currentTasks = tasks.ToList();

            var task = currentTasks.FirstOrDefault(t => t.Id == taskId);

            if (task == null) return "No tasks with such Id found..";

            var context = task.GetCurrentContext(false);

            if (context == null)
                return $"Task {taskId} stopped";

            var instanceId = context.TaskInstance.Id;

            contextsInWork.Add(instanceId, context);

            var view = await task.GetCurrentViewAsync(context);

            EndContextWork(instanceId);

            return string.IsNullOrEmpty(view)
                ? "Default dataset is empty.."
                : view;
        }

        public void DeleteTaskInstanceById(long taskInstanceId)
        {
            repository.DeleteEntity<DtoTaskInstance, long>(taskInstanceId);
            UpdateTaskList();
            SendServiceInfo($"Deleted task instance {taskInstanceId}");
        }


        public async Task<string> GetAllTaskInstancesJson(long taskId)
        {
            return JsonConvert.SerializeObject(await repository.GetAllTaskInstances(taskId));
        }

        public async Task<string> GetFullInstanceList_HtmlPageAsync(
            long taskId)
        {
            var instances = (await repository.GetAllTaskInstances(taskId))
                .Select(instance => new
                {
                    instance.Id,
                    instance.StartTime,
                    instance.Duration,
                    State = ((InstanceState) instance.State).ToString()
                });

            if (!instances.Any())
                return $"There are no executions of task {taskId} in the database";

            var pack = packageBuilder.GetPackage(instances);


            return await Task.Factory.StartNew(() =>
                tableView.ExecuteHtml($"Task {taskId} executions history", pack));
        }

        public List<DtoOperInstance> GetOperInstancesByTaskInstanceId(long id)
        {
            return repository
                .GetTaskOperInstances(id);
        }

        public List<DtoOperInstance> GetFullTaskOperInstances(long id)
        {
            return repository
                .GetFullTaskOperInstances(id);
        }

        public DtoOperInstance GetFullOperInstanceById(long id)
        {
            return repository.GetFullOperInstanceById(id);
        }

        public string GetAllRegisteredImportersJson()
        {
            return JsonConvert.SerializeObject(RegisteredImporters
                .ToDictionary(pair => pair.Key, pair => pair.Value.Name));
        }

        public string GetAllRegisteredExportersJson()
        {
            return JsonConvert.SerializeObject(RegisteredExporters
                .ToDictionary(pair => pair.Key, pair => pair.Value.Name));
        }

        public string GetAllB2BExportersJson(string keyParameter)
        {
            var taskList = repository.GetListEntitiesByDtoType<DtoTask>()
                .Select(task => new
                {
                    task.Id,
                    KeyParameter = JsonConvert
                        .DeserializeObject<Dictionary<string, object>>(task.Parameters)
                        .FirstOrDefault(kvp => kvp.Key == keyParameter).Value
                })
                .Where(values => values.KeyParameter != null);

            var exporters = operations
                .Where(oper => oper.ImplementationType == "CommonB2BExporter").ToList()
                .Select(oper => new
                {
                    oper.Id,
                    config = JsonConvert.DeserializeObject<B2BExporterConfig>(oper.Config),
                    oper.TaskId
                })
                .Join(taskList,
                    exp => exp.TaskId,
                    task => task.Id,
                    (exp, task) => new
                    {
                        exp.Id,
                        task.KeyParameter,
                        exp.config.ReportName,
                        exp.config.Description
                    });

            return JsonConvert.SerializeObject(exporters);
        }

        //public int CreateTaskByTemplate(ApiTask newTask) //todo: work with syncserver
        //{
        //    throw new NotImplementedException();
        //}

        public async Task<bool> StopTaskInstanceAsync(long taskInstanceId)
        {
            if (!contextsInWork.ContainsKey(taskInstanceId))
                return false;

            await Task.Factory.StartNew(() => CancelContextWork(taskInstanceId));

            return true;
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

            if (chatId == 0 || telegramChannels
                    .Select(channel => channel.ChatId).Contains(chatId))
                return;
            {
                DtoTelegramChannel channel =
                    new DtoTelegramChannel
                    {
                        ChatId = chatId,
                        Name = string.IsNullOrEmpty(chatName) ? "NoName" : chatName,
                        Type = (int) chatType
                    };

                channel.Id = (int)repository.CreateEntity(channel);
                UpdateDtoEntitiesList(telegramChannels);
            }
        }

        public void UpdateTaskRecord(DtoTask task)
        {
            repository.UpdateEntity(task);
            UpdateTaskList();
        }

        private void SendServiceInfo(string msg)
        {
            monik.ApplicationInfo(msg);
            Console.WriteLine(msg);
        }

        public long CreateRequestTaskInfo(TaskRequestInfo taskRequestInfo)
        {
            var newTaskRequestInfoId = repository.CreateTaskRequestInfo(taskRequestInfo);

            UpdateDtoEntitiesList(taskRequestInfos);
            SendServiceInfo($"Created TaskRequestInfo {newTaskRequestInfoId}");
            return newTaskRequestInfoId;
        }

        private bool isTaskNeedToRun(IReportTask task)
        {
            string[] cronStrings =
                       schedules.First(s => s.Id == task.Schedule.Id).Schedule.Split(';');

            foreach (var cronString in cronStrings)
            {
                var cronSchedule = CrontabSchedule.TryParse(cronString);

                if (cronSchedule == null)
                    continue;

                var occurrences =
                    cronSchedule.GetNextOccurrences(task.LastTime, DateTime.Now);

                if (!occurrences.Any())
                    continue;

                if (contextsInWork.Select(cont => cont.Value)
                    .Where(rtask => rtask.TaskId == task.Id).Any())
                    continue;

                return true;
            }
            return false;
        }

        public void UpdateTaskRequestInfo(TaskRequestInfo taskRequestInfo)
        {
            repository.UpdateEntity(taskRequestInfo);
            UpdateDtoEntitiesList(taskRequestInfos);
        }

        public ParameterMapping[] MapParameters(
            TaskParameter[] userParameters,
            ParameterInfo[] taskParameters)
        {
            var mapResult = new List<ParameterMapping>();

            foreach (var param in taskParameters)
            {
                var userParameter = userParameters.FirstOrDefault(up => up.Name.Equals(param.Name, StringComparison.InvariantCultureIgnoreCase));

                var mapParameter = new ParameterMapping(
                    param,
                    userParameter,
                    new List<string>(),
                    new object()
                    );

                if (userParameter == null)
                {
                    if (param.IsRequired)
                    {
                        mapParameter.Error.Add($"The required parameter with name:{param.Name} is missing.");
                    }
                    mapResult.Add(mapParameter);
                    continue;
                }

                var paramType = param.Type;
                switch (paramType)
                {
                    case "bigint":
                        if (!long.TryParse(userParameter.Value, out _))
                            mapParameter.Error.Add($"Wrong value input. TypeError in parameter: {userParameter.Name}.");
                        else
                            mapParameter.Value = Convert.ToInt64(userParameter.Value);
                        break;

                    case "int":
                        if (!int.TryParse(userParameter.Value, out _))
                            mapParameter.Error.Add($"Wrong value input. TypeError in parameter: {userParameter.Name}.");
                        else
                            mapParameter.Value = Convert.ToInt32(userParameter.Value);
                        break;

                    case "datetime":
                        if (!DateTime.TryParse(userParameter.Value, out _))
                            mapParameter.Error.Add($"Wrong value input. TypeError in parameter: {userParameter.Name}.");
                        else
                            mapParameter.Value = Convert.ToDateTime(userParameter.Value);
                        break;

                    case "string":
                        if (userParameter.GetType() != typeof(string))
                            mapParameter.Error.Add($"Wrong value input. TypeError in parameter: {userParameter.Name}.");
                        else
                            mapParameter.Value = userParameter.Value;
                        break;

                    default:
                        mapParameter.Error.Add($"Wrong type of parameter: {param.Name}.");
                        break;
                }
                mapResult.Add(mapParameter);
            }
            return mapResult.ToArray();
        }

        public List<IReportTask> GetTasksFromDb(long[] taskIds)
        {
            var dtoTasks = repository.GetTasksFromDb(taskIds);
            var curTasks = new List<IReportTask>();
            
            foreach (var dtoTask in dtoTasks)
            {
                var task = autofac.Resolve<IReportTask>(
                    new NamedParameter("id", dtoTask.Id),
                    new NamedParameter("name", dtoTask.Name),
                    new NamedParameter("parameters", dtoTask.Parameters),
                    new NamedParameter("dependsOn", dtoTask.DependsOn),
                    new NamedParameter("schedule", schedules
                        .FirstOrDefault(s => s.Id == dtoTask.ScheduleId)),
                    new NamedParameter("opers", operations
                        .Where(oper => oper.TaskId == dtoTask.Id)
                        .Where(oper => !oper.IsDeleted).ToList()),
                    new NamedParameter("parameterInfos", dtoTask.ParameterInfos));
                
                curTasks.Add(task);
            }
            return curTasks;
        }

        public IReportTask GetTaskFromDb(long taskId)
        {
            var dtoTask = repository.GetTaskFromDb(taskId);
            var curTask = autofac.Resolve<IReportTask>(
                new NamedParameter("id", dtoTask.Id),
                new NamedParameter("name", dtoTask.Name),
                new NamedParameter("parameters", dtoTask.Parameters),
                new NamedParameter("dependsOn", dtoTask.DependsOn),
                new NamedParameter("schedule", schedules
                    .FirstOrDefault(s => s.Id == dtoTask.ScheduleId)),
                new NamedParameter("opers", operations
                    .Where(oper => oper.TaskId == dtoTask.Id)
                    .Where(oper => !oper.IsDeleted).ToList()),
                new NamedParameter("parameterInfos", dtoTask.ParameterInfos));

            return curTask;
        }

        public List<IReportTask> GetAllTaskFromDb()
        {
            var dtoTasks = repository.GetListEntitiesByDtoType<DtoTask>();
            var curTasks = new List<IReportTask>();
            
            foreach (var dtoTask in dtoTasks)
            {
                var task = autofac.Resolve<IReportTask>(
                    new NamedParameter("id", dtoTask.Id),
                    new NamedParameter("name", dtoTask.Name),
                    new NamedParameter("parameters", dtoTask.Parameters),
                    new NamedParameter("dependsOn", dtoTask.DependsOn),
                    new NamedParameter("schedule", schedules
                        .FirstOrDefault(s => s.Id == dtoTask.ScheduleId)),
                    new NamedParameter("opers", operations
                        .Where(oper => oper.TaskId == dtoTask.Id)
                        .Where(oper => !oper.IsDeleted).ToList()),
                    new NamedParameter("parameterInfos", dtoTask.ParameterInfos));
                
                curTasks.Add(task);
            }
            return curTasks;
        }
    } //class
}
