using AutoMapper;
using ReportService.Entities;
using ReportService.Interfaces.Operations;
using ReportService.Interfaces.Protobuf;
using ReportService.Interfaces.ReportTask;
using ReportService.Operations.DataImporters.Configurations;
using System.Collections.Generic;
using System.Data.Common;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace ReportService.Operations.DataImporters
{
    public abstract class BaseDbImporter: IOperation
    {
        public bool CreateDataFolder { get; set; }
        public CommonOperationProperties Properties { get; set; } = new CommonOperationProperties();
        public bool SendVoidPackageError;

        protected readonly IPackageBuilder packageBuilder;
        protected readonly ThreadSafeRandom rnd;
        protected const int TriesCount = 3;

        public string ConnectionString;
        public string Query;
        public int TimeOut;
        public List<string> DataSetNames;
        public string GroupNumbers;

        public BaseDbImporter(IMapper mapper, DbImporterConfig config,
          IPackageBuilder builder, ThreadSafeRandom rnd)
        {
            mapper.Map(config, this);
            mapper.Map(config, Properties);
            Properties.NeedSavePackage = true;
            packageBuilder = builder;
            this.rnd = rnd;
        }

        public abstract Task ExecuteAsync(IReportTaskRunContext taskContext);

        protected virtual void FillPackage(DbDataReader reader, IReportTaskRunContext taskContext)
        {
            var pack = packageBuilder.GetPackage(reader, GroupNumbers);

            if (SendVoidPackageError && !pack.DataSets.Any())
                throw new InvalidDataException("No datasets obtaned during import");

            for (int i = 0; i < DataSetNames.Count; i++)
            {
                if (pack.DataSets.ElementAtOrDefault(i) != null)
                    pack.DataSets[i].Name = DataSetNames[i];
            }

            taskContext.Packages[Properties.PackageName] = pack;
        }
    }
}
