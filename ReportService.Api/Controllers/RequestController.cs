﻿using Microsoft.AspNetCore.Mvc;
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
        private const string GetTaskStatusRoute = "status/{id}";
        private const string RunTaskRoute = "runTask/";
        private const string GetTaskInfoRoute = "getTaskInfo/";
        private const string DownloadOpersResultRoute = "downloadOpersResult/";
        private const string StopTaskInstanceRoute = "stopTaskInstance/";

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
            var currentTask = logic.GetAllTasksJson().SingleOrDefault(t => t.Id == newParameters.TaskId);
            if (currentTask == null)
            {
                return GetNotFoundErrorResult(JsonConvert.SerializeObject(new Errors { ErrorsInfo = new Dictionary<string, string[]> { ["Task Error"] = new[] { $"Task with id {newParameters.TaskId} doesn't exist." } } }));
            }

            if (currentTask.ParameterInfos.Count() == 0)
            {
                return GetNotFoundErrorResult(JsonConvert.SerializeObject(new Errors { ErrorsInfo = new Dictionary<string, string[]> { ["Task Error"] = new[] { $"There is not task parameters in task ({currentTask.Id}:{currentTask.Name})" } } }));
            }

            var mapParameters = logic.MapParameters(newParameters.Parameters, currentTask.ParameterInfos.ToArray());
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
            
            var taskRequestInfo = new Models.TaskRequestInfo(
                newParameters.TaskId,
                newParameters.Parameters
                );

            var test = mapper.Map<Entities.TaskRequestInfo>(taskRequestInfo);
            var requestId = logic.CreateRequestTaskInfo(test);
            taskRequestInfo.RequestId = requestId;

            return GetSuccessfulResult(JsonConvert.SerializeObject(taskRequestInfo));
        }

        [Authorize(Domain0Auth.Policy, Roles = "reporting.view")]
        [HttpPost(GetTaskInfoRoute)]
        public TaskInfo[] GetTaskInfo([FromBody] TaskInfoFilter filter)
        {
            var currentTasks = logic.GetAllTasksJson();
            var tasksByTaskIds = new HashSet<long>(filter.TaskIds);

            return currentTasks
                .Where(t => tasksByTaskIds.Contains(t.Id))
                .Select(t => new TaskInfo(t.Id, t.Name, t.ParameterInfos))
                .ToArray();
        }

        [Authorize(Domain0Auth.Policy, Roles = "reporting.view")]
        [HttpPost(DownloadOpersResultRoute)]
        public ContentResult DownloadOpersResult(TaskRequestInfoFilter taskRequestInfoFilter)
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
        [HttpPost(StopTaskInstanceRoute)]
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
                return (GetSuccessfulResult(JsonConvert.SerializeObject(currentTaskRequestInfo)));
            }
            try
            {
                var stopped = await logic.StopTaskInstanceAsync((long)currentTaskRequestInfo.TaskInstanceId);

                return GetSuccessfulResult(JsonConvert.SerializeObject(currentTaskRequestInfo));
            }
            catch
            {
                return GetInternalErrorResult();
            }
        }
    }
}