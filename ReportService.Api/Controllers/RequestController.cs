using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Domain0.Tokens;
using Microsoft.AspNetCore.Authorization;
using ReportService.Interfaces.Core;
using AutoMapper;
using ReportService.Entities;
using ReportService.Api.Models;
using ReportService.Interfaces.ReportTask;
using Newtonsoft.Json;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using Newtonsoft.Json.Linq;
using ReportService.Entities.Dto;
using RequestStatusFilter = ReportService.Api.Models.RequestStatusFilter;

namespace ReportService.Api.Controllers
{
    [Authorize(Domain0Auth.Policy, Roles = "reporting.view, reporting.stoprun,  reporting.edit")]
    [Route("api/v3/[controller]")]
    [ApiController]
    public class RequestController : BaseController
    {
        private readonly ILogic logic;
        private readonly IMapper mapper;
        private readonly IArchiver archiver;
        private const string GetTaskStatusRoute = "getStatus/";
        private const string RunTaskRoute = "runTask/";
        private const string GetTaskInfoRoute = "getTaskInfo/";
        private const string DownloadResultRoute = "downloadResult/";
        private const string CancelRequestRoute = "cancelRequest/";

        public RequestController(ILogic logic, IMapper mapper, IArchiver archiver)
        {
            this.logic = logic;
            this.mapper = mapper;
            this.archiver = archiver;
        }

        [Authorize(Domain0Auth.Policy, Roles = "reporting.stoprun, reporting.edit")]
        [HttpPost(RunTaskRoute)]
        public ContentResult RunTask([FromBody] RunTaskParameters newParameters)
        {
            var currentTask = logic.GetTaskFromDb(newParameters.TaskId);

            if (currentTask == null)
            {
                return GetNotFoundErrorResult(JsonConvert.SerializeObject(new Errors { ErrorsInfo = new Dictionary<string, string[]> { ["Task Error"] = new[] { $"Task with id {newParameters.TaskId} doesn't exist." } } }));
            }

            if (currentTask.ParameterInfos.Count() == 0)
            {
                return GetNotFoundErrorResult(JsonConvert.SerializeObject(new Errors { ErrorsInfo = new Dictionary<string, string[]> { ["Task Error"] = new[] { $"There is not task parameters in task ({currentTask.Id}:{currentTask.Name})" } } }));
            }

            var mapParameters = logic.MapParameters(newParameters.Parameters, currentTask.ParameterInfos.ToArray());
            var mapErrors = mapParameters.Where(mp => mp.Error != null && mp.Error.Any()).ToList();

            if (mapErrors.Any())
            {
                var errors = new Errors
                { 
                    ErrorsInfo = mapErrors
                        .ToDictionary(k => k.ParameterInfo.Name, v => v.Error.ToArray())
                };

                return GetBadRequestError(JsonConvert.SerializeObject(errors));
            }

            var taskRequestInfo = new Models.TaskRequestInfo(
                newParameters.TaskId,
                newParameters.Parameters
                );

            var mappedTaskRequestInfo = mapper.Map<Entities.TaskRequestInfo>(taskRequestInfo);
            var requestId = logic.CreateRequestTaskInfo(mappedTaskRequestInfo);
            taskRequestInfo.RequestId = requestId;

            return GetSuccessfulResult(JsonConvert.SerializeObject(taskRequestInfo));
        }

        [Authorize(Domain0Auth.Policy, Roles = "reporting.view")]
        [HttpPost(GetTaskInfoRoute)]
        public TaskInfo[] GetTaskInfo([FromBody] TaskInfoFilter filter)
        {
            var currentTasks = logic.GetAllTaskFromDb();
            var tasksByTaskIds = new HashSet<long>(filter.TaskIds);

            return currentTasks
                .Where(t => tasksByTaskIds.Contains(t.Id))
                .Select(t => new TaskInfo(t.Id, t.Name, t.ParameterInfos))
                .ToArray();
        }

        [Authorize(Domain0Auth.Policy, Roles = "reporting.view")]
        [HttpPost(DownloadResultRoute)]
        public ContentResult DownloadResult(TaskRequestInfoFilter taskRequestInfoFilter)
        {
            var currentTaskRequestInfo = logic.GetTaskRequestInfoById(taskRequestInfoFilter.TaskRequestInfoId);

            if (currentTaskRequestInfo == null)
                return GetNotFoundErrorResult(JsonConvert.SerializeObject(
                    new Errors { ErrorsInfo = new Dictionary<string, string[]> { ["NotFoundError"] = new[] { $"The TaskRequestInfo with ID: {taskRequestInfoFilter.TaskRequestInfoId} was not found." } } })
                    );

            if (currentTaskRequestInfo.TaskInstanceId == null)
                return null;

            var fullOperInstances = logic.GetFullTaskOperInstances((long)currentTaskRequestInfo.TaskInstanceId);

            var data = fullOperInstances.Select(opi => 
                new Models.DataSet { 
                    OperationInstanceId = opi.OperationId, 
                    StartTime = opi.StartTime, 
                    Data = archiver.ExtractFromByteArchive(opi.DataSet)
                }).Where(dt => dt.Data != null).ToArray();

            return GetSuccessfulResult(JsonConvert.SerializeObject(data));
        }

        [Authorize(Domain0Auth.Policy, Roles = "reporting.stoprun, reporting.edit")]
        [HttpPost(CancelRequestRoute)]
        public async Task<ContentResult> CancelRequest([FromBody] TaskRequestInfoFilter taskRequestInfoFilter)
        {
            var currentTaskRequestInfo = logic.GetTaskRequestInfoById(taskRequestInfoFilter.TaskRequestInfoId);

            if (currentTaskRequestInfo == null)
            {
                return GetNotFoundErrorResult($"TaskRequestInfo with id {taskRequestInfoFilter.TaskRequestInfoId} not found.");
            }

            if (currentTaskRequestInfo.TaskInstanceId == null)
            {
                currentTaskRequestInfo.Status = (int)RequestStatus.Canceled;
                logic.UpdateTaskRequestInfo(currentTaskRequestInfo);
                return (GetSuccessfulResult(JsonConvert.SerializeObject(mapper.Map<Models.TaskRequestInfo>(currentTaskRequestInfo))));
            }
            try
            {
                var stopped = await logic.StopTaskInstanceAsync((long)currentTaskRequestInfo.TaskInstanceId);
                currentTaskRequestInfo.Status = (int)RequestStatus.Canceled;
                return GetSuccessfulResult(JsonConvert.SerializeObject(mapper.Map<Models.TaskRequestInfo>(currentTaskRequestInfo)));
            }
            catch
            {
                return GetInternalErrorResult();
            }
        }

        [HttpPost(GetTaskStatusRoute)]
        public ContentResult GetTaskStatus([FromBody] RequestStatusFilter requestStatusFilter)
        {
            requestStatusFilter.TimePeriod?.UpdateTimeDifferenc();

            if (requestStatusFilter.TaskIds == null && requestStatusFilter.TaskRequestInfoIds == null)
                return GetBadRequestError(JsonConvert.SerializeObject(
                    new Errors { ErrorsInfo = new Dictionary<string, string[]> { ["BadRequestError"] = new[] { "Task Ids or TaskRequestInfoIds must be set." } } }
                    ));

            if (requestStatusFilter.TimePeriod?.timeDifference < 0)
                return GetBadRequestError(JsonConvert.SerializeObject(
                    new Errors { ErrorsInfo = new Dictionary<string, string[]> { ["BadRequestError"] = new[] { "Wrong time period." } } }
                    ));

            if (requestStatusFilter.TimePeriod?.timeDifference > 90)
                return GetBadRequestError(JsonConvert.SerializeObject(
                    new Errors { ErrorsInfo = new Dictionary<string, string[]> { ["BadRequestError"] = new[] { "The time period is too big." } } }
                    ));

            var currentTaskRequestInfoByFilter = logic.GetTaskRequestInfoByFilter(mapper.Map<Entities.Dto.RequestStatusFilter>(requestStatusFilter));
            var taskRequestInfos = mapper.Map<Models.TaskRequestInfo[]>(currentTaskRequestInfoByFilter);
            
            return GetSuccessfulResult(JsonConvert.SerializeObject(taskRequestInfos));
        }
    }
}
