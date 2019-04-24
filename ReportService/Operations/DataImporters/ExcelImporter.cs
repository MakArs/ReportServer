using System.IO;
using System.Threading.Tasks;
using AutoMapper;
using OfficeOpenXml;
using ReportService.Interfaces.Core;
using ReportService.Interfaces.Operations;
using ReportService.Interfaces.Protobuf;
using ReportService.Interfaces.ReportTask;

namespace ReportService.Operations.DataImporters
{
    public class ExcelPackageReadingParameters
    {
        public string SheetName;
        public bool SkipEmptyRows;
        public string[] ColumnList;
        public bool UseColumnNames;
        public int FirstDataRow;
        public int MaxRowCount;
    }

    public class ExcelImporter : IOperation
    {
        private readonly IPackageBuilder packageBuilder;
        public ExcelPackageReadingParameters ExcelParameters;

        public CommonOperationProperties Properties { get; set; } = new CommonOperationProperties();

        public string FileFolder;
        public string FileName;

        public ExcelImporter(IMapper mapper, ExcelImporterConfig config, IPackageBuilder builder)
        {
            mapper.Map(config, this);
            mapper.Map(config, Properties);
            Properties.NeedSavePackage = true;
            ExcelParameters = new ExcelPackageReadingParameters();
            mapper.Map(config, ExcelParameters);
            packageBuilder = builder;
        }

        public void Execute(IRTaskRunContext taskContext)
        {
            var fullPath = Path.Combine(FileFolder == "Default folder" ? taskContext.DataFolderPath :
                FileFolder, FileName);

            var fi = new FileInfo(fullPath);

            using (var pack = new ExcelPackage(fi))
            {
                var package = packageBuilder.GetPackage(pack, ExcelParameters);
                taskContext.Packages[Properties.PackageName] = package;
            }
        }

        public Task ExecuteAsync(IRTaskRunContext taskContext) // todo: cancellation if needed
        {
            Execute(taskContext);
            return Task.CompletedTask;
        }
    }
}