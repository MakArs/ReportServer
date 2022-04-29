using AutoMapper;
using Newtonsoft.Json;
using ReportService.Entities.Dto;
using ReportService.Api.Controllers;
using Newtonsoft.Json.Linq;
using ReportService.Entities;

namespace ReportService.Api.Models
{
    public class ModelsMapperProfile : Profile
    {
        public ModelsMapperProfile()
        {
            CreateMap<ReportTask.ReportTask, ApiTask>()
                .ForMember("ScheduleId", opt => opt.MapFrom(s => s.Schedule.Id))
                .ForMember(m => m.Parameters, opt =>
                    opt.MapFrom(s => JsonConvert.SerializeObject(s.Parameters)))
                .ForMember("ParameterInfos", opt => 
                    opt.MapFrom(s => JsonConvert.SerializeObject(s.ParameterInfos)));

            CreateMap<ApiTask, DtoTask>()
                .ForMember("DependsOn", opt =>
                    opt.MapFrom(s =>
                        s.DependsOn == null
                            ? null
                            : JsonConvert.SerializeObject(s.DependsOn)));

            CreateMap<DtoOperInstance, ApiOperInstance>()
                .ForMember("DataSet", opt => opt.Ignore());

            CreateMap<TaskRequestInfo, Entities.TaskRequestInfo>()
                .ForMember("Parameters", opt => opt.MapFrom(s => JsonConvert.SerializeObject(s.Parameters)))
                .ReverseMap()
                .ForPath(m => m.Parameters, opt => 
                    opt.MapFrom(s => JsonConvert.DeserializeObject<Entities.TaskParameter[]>(s.Parameters)));

            CreateMap<TimePeriod, Entities.Dto.TimePeriod>();

            CreateMap<RequestStatusFilter, Entities.Dto.RequestStatusFilter>()
                .ForMember("TimePeriod", opt => opt.MapFrom(s => s.TimePeriod));
        }
    }
}
