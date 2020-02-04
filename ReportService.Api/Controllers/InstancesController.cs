using AutoMapper;
using Domain0.Tokens;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using ReportService.Api.Models;
using ReportService.Interfaces.Core;
using System.Linq;

namespace ReportService.Api.Controllers
{
    [Authorize(Domain0Auth.Policy, Roles = "reporting.view, reporting.stoprun, reporting.edit")]
    [Route("api/v3/[controller]")]
    [ApiController]
    public class InstancesController : BaseController
    {
        private readonly ILogic logic;
        private readonly IMapper mapper;
        private readonly IArchiver archiver;
        private const string GetOperInstancesOfTaskInstanceRoute = "{taskInstanceid}/operinstances";
        private const string GetFullOperInstanceRoute = "operinstances/{operInstanceid}";
        private const string DeleteTaskInstanceRoute = "taskinstances/{taskInstanceid}";

        public InstancesController(ILogic logic, IMapper mapper, IArchiver archiver)
        {
            this.logic = logic;
            this.mapper = mapper;
            this.archiver = archiver;
        }

        [HttpGet(GetOperInstancesOfTaskInstanceRoute)]
        public ContentResult GetOperInstancesOfTaskInstance(long taskInstanceid)
        {
            try
            {
                var instances = logic
                    .GetOperInstancesByTaskInstanceId(taskInstanceid);

                var apiInstances = instances.Select(inst =>
                    mapper.Map<ApiOperInstance>(inst)).ToList();

                return GetSuccessfulResult(JsonConvert.SerializeObject(apiInstances));
            }
            catch
            {
                return GetInternalErrorResult();
            }
        }

        [HttpGet(GetFullOperInstanceRoute)]
        public ContentResult GetFullOperInstance(long operInstanceid)
        {
            try
            {
                var instance = logic
                    .GetFullOperInstanceById(operInstanceid);

                var apiInstance = mapper.Map<ApiOperInstance>(instance);
                apiInstance.DataSet = archiver.ExtractFromByteArchive(instance.DataSet);

                return GetSuccessfulResult(JsonConvert.SerializeObject(apiInstance));
            }
            catch
            {
                return GetInternalErrorResult();
            }
        }

        [Authorize(Domain0Auth.Policy, Roles = "reporting.edit")]
        [HttpDelete(DeleteTaskInstanceRoute)]
        public IActionResult DeleteTaskInstance(long taskInstanceid)
        {
            try
            {
                logic.DeleteTaskInstanceById(taskInstanceid);
                return StatusCode(200);
            }
            catch
            {
                return StatusCode(500);
            }
        }
    }
}