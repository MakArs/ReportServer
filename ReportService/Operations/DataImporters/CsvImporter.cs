using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using CsvHelper;
using CsvHelper.Configuration;
using ReportService.Interfaces.Core;
using ReportService.Interfaces.Protobuf;
using ReportService.Interfaces.ReportTask;

namespace ReportService.Operations.DataImporters
{
    public class CsvImporter : IOperation
    {
        public CommonOperationProperties Properties { get; set; } = new CommonOperationProperties();
        public int MaxRowCount;
        public string FilePath;

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
            using (var textReader = File.OpenText(FilePath))
            {
                using (var csvReader = new CsvReader(textReader))
                {
                    taskContext.Packages[Properties.PackageName] = packageBuilder.GetPackage(csvReader);
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