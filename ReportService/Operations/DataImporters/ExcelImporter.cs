using System.IO;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using OfficeOpenXml;
using ReportService.Entities;
using ReportService.Interfaces.Operations;
using ReportService.Interfaces.Protobuf;
using ReportService.Interfaces.ReportTask;
using ReportService.Operations.DataImporters.Configurations;

namespace ReportService.Operations.DataImporters
{
    public class ExcelImporter : IOperation
    {
        private readonly IPackageBuilder packageBuilder;
        public ExcelReadingParameters ExcelParameters;

        public CommonOperationProperties Properties { get; set; } = new CommonOperationProperties();
        public bool SendVoidPackageError;

        public string FileFolder;
        public string FileName;
        public string GroupNumbers;

        public ExcelImporter(IMapper mapper, ExcelImporterConfig config, IPackageBuilder builder)
        {
            mapper.Map(config, this);
            mapper.Map(config, Properties);
            Properties.NeedSavePackage = true;
            ExcelParameters = new ExcelReadingParameters();
            mapper.Map(config, ExcelParameters);
            packageBuilder = builder;
        }

        private void Execute(IReportTaskRunContext taskContext)
        {
            var fullPath = Path.Combine(FileFolder == "Default folder" ? taskContext.DataFolderPath : FileFolder,
                FileName);

            var fi = new FileInfo(fullPath);

            using var pack = new ExcelPackage(fi);

            var package = packageBuilder.GetPackage(pack, ExcelParameters, GroupNumbers);

            if (SendVoidPackageError && !package.DataSets.Any())
                throw new InvalidDataException("No datasets obtaned during import");

            taskContext.Packages[Properties.PackageName] = package;
        }

        public Task ExecuteAsync(IReportTaskRunContext taskContext) // todo: cancellation if needed
        {
            Execute(taskContext);
            return Task.CompletedTask;
        }
    }
}