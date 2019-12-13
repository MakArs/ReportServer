using System;
using AutoMapper;
using Newtonsoft.Json;

namespace ReportService.Core
{
    public class MapperProfile : Profile
    {
        public MapperProfile()
        {
            //CreateMap<DtoRecepientGroup, RecipientGroup>();

            //CreateMap<ReportTask.ReportTask, DtoTask>()
            //    .ForMember("ScheduleId", opt => opt.MapFrom(s => s.Schedule.Id))
            //    .ForMember("Parameters", opt =>
            //        opt.MapFrom(s => JsonConvert.SerializeObject(s.Parameters)))
            //    .ForMember("DependsOn", opt =>
            //        opt.MapFrom(s => JsonConvert.SerializeObject(s.DependsOn)));

            //CreateMap<ReportTask.ReportTask, ApiTask>()
            //    .ForMember("ScheduleId", opt => opt.MapFrom(s => s.Schedule.Id))
            //    .ForMember("Parameters", opt =>
            //        opt.MapFrom(s => JsonConvert.SerializeObject(s.Parameters)));

            //CreateMap<ApiTask, DtoTask>()
            //    .ForMember("DependsOn", opt =>
            //        opt.MapFrom(s =>
            //            s.DependsOn == null
            //                ? null
            //                : JsonConvert.SerializeObject(s.DependsOn)));

            //CreateMap<DtoOperInstance, ApiOperInstance>()
            //    .ForMember("DataSet", opt => opt.Ignore());

            //CreateMap<DtoOperInstance, DtoTaskInstance>();

            //CreateMap<DtoOperation, CommonOperationProperties>();

            //CreateMap<DbExporterConfig, DbExporter>();
            //CreateMap<DbExporterConfig, CommonOperationProperties>();
            //CreateMap<EmailExporterConfig, EmailDataSender>();
            //CreateMap<EmailExporterConfig, CommonOperationProperties>();
            //CreateMap<TelegramExporterConfig, TelegramDataSender>();
            //CreateMap<TelegramExporterConfig, CommonOperationProperties>();
            //CreateMap<B2BExporterConfig, B2BExporter>();
            //CreateMap<B2BExporterConfig, CommonOperationProperties>();
            //CreateMap<DbImporterConfig, DbImporter>()
            //    .ForMember("DataSetNames", opt =>
            //        opt.MapFrom(s => s.DataSetNames.Split(new[] { ';' },
            //                StringSplitOptions.RemoveEmptyEntries)
            //            .Where(name => !string.IsNullOrWhiteSpace(name))
            //            .ToList()));
            //CreateMap<DbImporterConfig, CommonOperationProperties>();
            //CreateMap<ExcelImporterConfig, ExcelImporter>();
            //CreateMap<ExcelImporterConfig, CommonOperationProperties>();
            //CreateMap<ExcelImporterConfig, ExcelReadingParameters>();
            //CreateMap<ExcelImporterConfig, CommonOperationProperties>();
            //CreateMap<CsvImporterConfig, CsvImporter>();
            //CreateMap<CsvImporterConfig, CommonOperationProperties>();
            //CreateMap<SshImporterConfig, SshImporter>();
            //CreateMap<SshImporterConfig, CommonOperationProperties>();
            //CreateMap<SshExporterConfig, SshExporter>();
            //CreateMap<SshExporterConfig, CommonOperationProperties>();
            //CreateMap<FtpExporterConfig, FtpExporter>();
            //CreateMap<FtpExporterConfig, CommonOperationProperties>();
            //CreateMap<HistoryImporterConfig, HistoryImporter>();
            //CreateMap<HistoryImporterConfig, CommonOperationProperties>();
        }
    }
}
