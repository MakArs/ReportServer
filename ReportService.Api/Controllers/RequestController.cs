using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Domain0.Tokens;
using Microsoft.AspNetCore.Authorization;
using ReportService.Interfaces.Core;
using AutoMapper;
using ReportService.Entities.Dto;
using ReportService.Entities;
using ReportService.Api.Models;
using ReportService.Interfaces.ReportTask;
using Newtonsoft.Json;

namespace ReportService.Api.Controllers
{
    [Authorize(Domain0Auth.Policy, Roles = "reporting.view, reporting.stoprun,  reporting.edit")]
    [Route("api/v3/[controller]")]
    [ApiController]
    public class RequestController : BaseController
    {
        private readonly ILogic logic;
        private readonly IMapper mapper;
        private const string GetTaskStatusRoute = "status/{id}";
        private const string RunTaskRoute = "runTask/";
        private const string GetTaskInfoRoute = "getTaskInfo/";

        public RequestController(ILogic logic, IMapper mapper)
        {
            this.logic = logic;
            this.mapper = mapper;
        }

        //[HttpPost(GetTaskStatusRoute)]
        //public TaskRequestInfo[] GetTaskRequestInfo() { return null; }

        [Authorize(Domain0Auth.Policy, Roles = "reporting.stoprun, reporting.edit")]
        [HttpPost(RunTaskRoute)]
        public ContentResult RunTask([FromBody] RunTaskParameters newParameters) 
        {
            var currentTask = logic.GetAllTasksJson().SingleOrDefault(t => t.Id == newParameters.TaskId);
            if (currentTask == null)
            {
                return GetNotFoundErrorResult(JsonConvert.SerializeObject(new Errors { ErrorsInfo = new Dictionary<string, string[]> { ["Task Error"] = new[] { $"Task with id {newParameters.TaskId} doesn't exist." } } }));
            }

            if (currentTask.ParameterInfos.Count() == 0)
            {
                return GetNotFoundErrorResult(JsonConvert.SerializeObject(new Errors { ErrorsInfo = new Dictionary<string, string[]> { ["Task Error"] = new[] { $"There is not task parameters in task ({currentTask.Id}:{currentTask.Name})" } } }));
            }

            var mapParameters = MapParameters(newParameters.Parameters, currentTask.ParameterInfos.ToArray());
            var mapErrors = mapParameters.Where(mp => mp.Error.Any());

            if (mapErrors.Any())
            {
                var errors = new Errors
                { 
                    ErrorsInfo = mapErrors
                        .ToDictionary(k => k.ParameterInfo.Name, v => v.Error.ToArray())
                };

                return GetBadRequestError(JsonConvert.SerializeObject(errors));

            }

            DateTime timeFrom = DateTime.MinValue;
            DateTime timeTo = DateTime.MinValue;
            
            foreach (var parameter in mapParameters)
            {
                if (parameter.ParameterInfo.Name.ToLower().Contains("repparfrom"))
                    timeFrom = Convert.ToDateTime(parameter.UserValue.Value);
                else if (parameter.ParameterInfo.Name.ToLower().Contains("repparto"))
                    timeTo = Convert.ToDateTime(parameter.UserValue.Value);
            }


            if (timeTo.Subtract(timeFrom).Days > 30)
                return GetBadRequestError(JsonConvert.SerializeObject(
                    new Errors { ErrorsInfo = new Dictionary<string, string[]> { ["Time Period Error"] = new[] { $"The time period is to big ({timeTo.Subtract(timeFrom).Days} days)." } } })
                    );
            else if (timeTo.Subtract(timeFrom).Days < 0)
                return GetBadRequestError(JsonConvert.SerializeObject(
                    new Errors { ErrorsInfo = new Dictionary<string, string[]> { ["Time Period Error"] = new[] { $"RepParFrom ({timeFrom}) is bigger than RepParTo ({timeTo})." } } })
                    );
            
            var taskRequestInfo = new TaskRequestInfo(
                newParameters.TaskId,
                newParameters.Parameters
                );

            var test = mapper.Map<Entities.TaskRequestInfo>(taskRequestInfo);
            var requestId = logic.CreateRequestTaskInfo(test);
            taskRequestInfo.RequestId = requestId;

            return GetSuccessfulResult(JsonConvert.SerializeObject(taskRequestInfo));
        }

        private ParameterMapping[] MapParameters(
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


        [Authorize(Domain0Auth.Policy, Roles = "reporting.view")]
        [HttpPost(GetTaskInfoRoute)]
        public TaskInfo[] GetTaskInfo([FromBody]  TaskInfoFilter filter)
        {
            var currentTasks = logic.GetAllTasksJson();
            var tasksByTaskIds = new HashSet<long>(filter.TaskIds);

            return currentTasks
                .Where(t => tasksByTaskIds.Contains(t.Id))
                .Select(t => new TaskInfo(t.Id, t.Name, t.ParameterInfos))
                .ToArray();
        }
    }

    public class TaskInfoFilter
    {
        public long[] TaskIds { get; set; }
    }

    public class RunTaskParameters
    {
        public long TaskId { get; set; }
        public TaskParameter[] Parameters { get; set; }
    }

    public class TaskInfo
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public ParameterInfo[] ParameterInfos { get; set; }

        public TaskInfo(long id, string name, List<ParameterInfo> parameterInfos)
        {
            this.Id = id;
            this.Name = name;
            this.ParameterInfos = parameterInfos.ToArray();
        }
    }

    public class TaskRequestInfo
    {
        public long? RequestId { get; set; }
        public long TaskId { get; set; }
        public long? TaskInstanceId { get; set; }
        public TaskParameter[] Parameters { get; set; }
        public DateTime CreateTime { get; set; }
        public DateTime UpdateTime { get; set; }
        public RequestStatus Status { get; set; }

        public TaskRequestInfo(long taskId, TaskParameter[] parameters, RequestStatus Status = RequestStatus.Pending) 
        {
            this.TaskId = taskId;
            this.Parameters = parameters;
            this.CreateTime = DateTime.UtcNow;
            this.UpdateTime = DateTime.UtcNow;
            this.Status = Status;
        }
    }
    public class ParameterMapping
    {
        public ParameterInfo ParameterInfo { get; set; }
        public TaskParameter UserValue { get; set; }
        public List<string> Error { get; set; }
        public object Value { get; set; }

        public ParameterMapping(
            ParameterInfo parameterInfo, 
            TaskParameter taskParameter,
            List<string> error,
            object value
            )
        {
            this.ParameterInfo = parameterInfo;
            this.UserValue = taskParameter;
            this.Error = error;
            this.Value = value;
        }
    }

    public class Errors
    {
        public Dictionary<string, string[]> ErrorsInfo { get; set; }
    }
}