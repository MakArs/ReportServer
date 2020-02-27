using System;
using System.Linq;
using AutoMapper;
using Newtonsoft.Json;
using ReportService.Entities;
using ReportService.Entities.Dto;
using ReportService.Operations.DataExporters;
using ReportService.Operations.DataExporters.Configurations;
using ReportService.Operations.DataImporters;
using ReportService.Operations.DataImporters.Configurations;

namespace ReportService.Core
{
    public class MapperProfile : Profile
    {
        public MapperProfile()
        {
            CreateMap<DtoRecepientGroup, RecipientGroup>();

            CreateMap<ReportTask.ReportTask, DtoTask>()
                .ForMember("ScheduleId", opt => opt.MapFrom(s => s.Schedule.Id))
                .ForMember("Parameters", opt =>
                    opt.MapFrom(s => JsonConvert.SerializeObject(s.Parameters)))
                .ForMember("DependsOn", opt =>
                    opt.MapFrom(s => JsonConvert.SerializeObject(s.DependsOn)));

            CreateMap<DtoOperInstance, DtoTaskInstance>();

            CreateMap<DtoOperation, CommonOperationProperties>();

            CreateMap<DbExporterConfig, BaseDbExporter>();
            CreateMap<DbExporterConfig, CommonOperationProperties>();
            CreateMap<EmailExporterConfig, EmailDataSender>();
            CreateMap<EmailExporterConfig, CommonOperationProperties>();
            CreateMap<TelegramExporterConfig, TelegramDataSender>();
            CreateMap<TelegramExporterConfig, CommonOperationProperties>();
            CreateMap<B2BExporterConfig, B2BExporter>();
            CreateMap<B2BExporterConfig, CommonOperationProperties>();
            CreateMap<DbImporterConfig, BaseDbImporter>()
                .ForMember("DataSetNames", opt =>
                    opt.MapFrom(s => s.DataSetNames.Split(new[] {';'},
                            StringSplitOptions.RemoveEmptyEntries)
                        .Where(name => !string.IsNullOrWhiteSpace(name))
                        .ToList()));
            CreateMap<DbImporterConfig, CommonOperationProperties>();
            CreateMap<ExcelImporterConfig, ExcelImporter>();
            CreateMap<ExcelImporterConfig, CommonOperationProperties>();
            CreateMap<ExcelImporterConfig, ExcelReadingParameters>();
            CreateMap<ExcelImporterConfig, CommonOperationProperties>();
            CreateMap<CsvImporterConfig, CsvImporter>();
            CreateMap<CsvImporterConfig, CommonOperationProperties>();
            CreateMap<SshImporterConfig, SshImporter>();
            CreateMap<SshImporterConfig, CommonOperationProperties>();
            CreateMap<SshExporterConfig, SshExporter>();
            CreateMap<SshExporterConfig, CommonOperationProperties>();
            CreateMap<FtpExporterConfig, FtpExporter>();
            CreateMap<FtpExporterConfig, CommonOperationProperties>();
            CreateMap<HistoryImporterConfig, HistoryImporter>();
            CreateMap<HistoryImporterConfig, CommonOperationProperties>();
        }
    }
}