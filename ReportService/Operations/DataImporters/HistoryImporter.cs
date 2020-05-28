using System.Threading.Tasks;
using AutoMapper;
using ReportService.Entities;
using ReportService.Interfaces.Core;
using ReportService.Interfaces.Operations;
using ReportService.Interfaces.ReportTask;
using ReportService.Operations.DataImporters.Configurations;

namespace ReportService.Operations.DataImporters
{
    public class HistoryImporter : IOperation
    {
        public bool CreateDataFolder { get; set; }
        public CommonOperationProperties Properties { get; set; } = new CommonOperationProperties();
        private readonly IRepository repos;
        private readonly IArchiver archiver;
        public long OperInstanceId;

        public HistoryImporter(IMapper mapper, HistoryImporterConfig config,
            IRepository repository, IArchiver archiver)
        {
            mapper.Map(config, this);
            mapper.Map(config, Properties);
            Properties.NeedSavePackage = true;
            repos = repository;
            this.archiver = archiver;
        }

        public async Task ExecuteAsync(IReportTaskRunContext taskContext)
        {
            var query = $@"select ""DataSet"" from ""OperInstance"" with(nolock) where ""Id""={OperInstanceId}";

            var instance = await repos.GetBaseQueryResult(query, taskContext.CancelSource.Token);

            var package = OperationPackage.Parser.ParseFrom(archiver.ExtractFromByteArchive(instance as byte[]));
            taskContext.Packages[Properties.PackageName] = package;
        }
    }
}