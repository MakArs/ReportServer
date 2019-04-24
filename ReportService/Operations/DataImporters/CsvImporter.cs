using System.IO;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using CsvHelper;
using ReportService.Interfaces.Core;
using ReportService.Interfaces.Operations;
using ReportService.Interfaces.Protobuf;
using ReportService.Interfaces.ReportTask;

namespace ReportService.Operations.DataImporters
{
    public class CsvImporter : IOperation
    {
        public CommonOperationProperties Properties { get; set; } = new CommonOperationProperties();
        public string Delimiter;
        public string FileFolder;
        public string FileName;
        public string DataSetName;

        private readonly IPackageBuilder packageBuilder;

        public CsvImporter(IMapper mapper, CsvImporterConfig config, IPackageBuilder builder)
        {
            mapper.Map(config, this);
            mapper.Map(config, Properties);
            Properties.NeedSavePackage = true;
            packageBuilder = builder;
        }

        public void Execute(IRTaskRunContext taskContext)
        {
            var fullPath = Path.Combine(FileFolder == "Default folder" ? taskContext.DataFolderPath :
                FileFolder, FileName);

            using (var textReader =
                File.OpenText(fullPath))
            {
                using (var csvReader = new CsvReader(textReader))
                {
                    csvReader.Configuration.Delimiter = Delimiter;
                    var pack = packageBuilder.GetPackage(csvReader);
                    pack.DataSets.First().Name = DataSetName;

                    taskContext.Packages[Properties.PackageName] = pack;
                }
            }
        }

        public Task ExecuteAsync(IRTaskRunContext taskContext)
        {
            Execute(taskContext);
            return Task.CompletedTask;
        }
    }
}