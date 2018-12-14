using System.IO;
using System.Threading.Tasks;
using AutoMapper;
using OfficeOpenXml;
using ReportService.Interfaces.Core;
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

        public string FilePath;

        public ExcelImporter(IMapper mapper, ExcelImporterConfig config, IPackageBuilder builder)
        {
            mapper.Map(config, this);
            mapper.Map(config, Properties);
            ExcelParameters = new ExcelPackageReadingParameters();
            mapper.Map(config, ExcelParameters);
            packageBuilder = builder;
        }

        public void Execute(IRTaskRunContext taskContext)
        {
            var fi = new FileInfo(FilePath);

            using (var pack = new ExcelPackage(fi))
            {
                var package = packageBuilder.GetPackage(pack, ExcelParameters);
                taskContext.Packages[Properties.PackageName] = package;
            }
        }

        public async Task ExecuteAsync(IRTaskRunContext taskContext)
        {
            await Task.Run(() =>
            {
                var fi = new FileInfo(FilePath);

                using (var pack = new ExcelPackage(fi))
                {
                    var package = packageBuilder.GetPackage(pack, ExcelParameters);
                    taskContext.Packages[Properties.PackageName] = package;
                }
            });
        }
    }
}