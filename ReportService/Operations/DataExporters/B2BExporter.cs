using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Gerakul.FastSql.Common;
using Gerakul.FastSql.SqlServer;
using Google.Protobuf;
using ReportService.Interfaces.Core;
using ReportService.Interfaces.ReportTask;

namespace ReportService.Operations.DataExporters
{
    public class B2BExporter : IOperation
    {
        public CommonOperationProperties Properties { get; set; } = new CommonOperationProperties();
        public bool RunIfVoidPackage { get; set; }

        private readonly IArchiver archiver;

        public string ReportName;
        public string ConnectionString;
        public string ExportTableName;
        public string ExportInstanceTableName;
        public int DbTimeOut;

        public B2BExporter(IMapper mapper, IArchiver archiver,
            B2BExporterConfig config)
        {
            this.archiver = archiver;
            mapper.Map(config, this);
            mapper.Map(config, Properties);
        }

        public void Execute(IRTaskRunContext taskContext)
        {
            var package = taskContext.Packages[Properties.PackageName];

            if (!RunIfVoidPackage && package.DataSets.Count == 0)
                return;

            var context = SqlContextProvider.DefaultInstance
                .CreateContext(ConnectionString);
            

            if (context.CreateSimple($@" IF OBJECT_ID('{ExportTableName}') IS NOT NULL
                IF EXISTS(SELECT * FROM {ExportTableName} WHERE id = {taskContext.TaskId})
				AND OBJECT_ID('{ExportInstanceTableName}') IS NOT NULL
                AND  EXISTS(SELECT 1 FROM sys.columns 
				  WHERE Name = 'Id'
				  AND Object_ID = Object_ID('{ExportInstanceTableName}'))
				  AND  EXISTS(SELECT 1 FROM sys.columns 
				  WHERE Name = 'Created'
				  AND Object_ID = Object_ID('{ExportInstanceTableName}')) 
				  AND  EXISTS(SELECT 1 FROM sys.columns 
				  WHERE Name = 'ReportId'
				  AND Object_ID = Object_ID('{ExportInstanceTableName}')) 
				  AND  EXISTS(SELECT 1 FROM sys.columns 
				  WHERE Name = 'DataPackage'
				  AND Object_ID = Object_ID('{ExportInstanceTableName}'))  
                SELECT 1
                ELSE SELECT 0").ExecuteQueryFirstColumn<int>().First() != 1)
                return;

            byte[] archivedPackage;

            using (var stream = new MemoryStream())
            {
                package.WriteTo(stream);
                archivedPackage = archiver.CompressStream(stream);
            }

            var newInstance = new
            {
                ReportId = taskContext.TaskId,
                ExecuteTime = DateTime.Now,
                OperationPackage = archivedPackage
            };

            context.Insert(ExportInstanceTableName, newInstance, new QueryOptions(DbTimeOut), "Id");
        }

        public async Task ExecuteAsync(IRTaskRunContext taskContext)
        {
            var package = taskContext.Packages[Properties.PackageName];

            if (!RunIfVoidPackage && package.DataSets.Count == 0)
                return;

            var context = SqlContextProvider.DefaultInstance
                .CreateContext(ConnectionString);


            //Properties.Id = 3;
            //taskContext.TaskId = 3;

            if (await context.CreateSimple($@"IF OBJECT_ID('{ExportTableName}') IS NOT NULL
                IF EXISTS(SELECT * FROM {ExportTableName} WHERE id = {taskContext.TaskId})
				AND OBJECT_ID('{ExportInstanceTableName}') IS NOT NULL
                AND  EXISTS(SELECT 1 FROM sys.columns 
				  WHERE Name = 'Id'
				  AND Object_ID = Object_ID('{ExportInstanceTableName}'))
				  AND  EXISTS(SELECT 1 FROM sys.columns 
				  WHERE Name = 'Created'
				  AND Object_ID = Object_ID('{ExportInstanceTableName}')) 
				  AND  EXISTS(SELECT 1 FROM sys.columns 
				  WHERE Name = 'ReportID'
				  AND Object_ID = Object_ID('{ExportInstanceTableName}')) 
				  AND  EXISTS(SELECT 1 FROM sys.columns 
				  WHERE Name = 'DataPackage'
				  AND Object_ID = Object_ID('{ExportInstanceTableName}'))  
                SELECT 1
                ELSE SELECT 0").ExecuteQueryFirstColumnAsync<int>().First() != 1)
                return;

            byte[] archivedPackage;

            using (var stream = new MemoryStream())
            {
                package.WriteTo(stream);
                archivedPackage = archiver.CompressStream(stream);
            }

            var newInstance = new
            {
                ReportID = taskContext.TaskId,
                Created = DateTime.Now,
                DataPackage = archivedPackage
            };

            await context.InsertAsync(ExportInstanceTableName, newInstance, new QueryOptions(DbTimeOut), "Id");
        }
    }
}