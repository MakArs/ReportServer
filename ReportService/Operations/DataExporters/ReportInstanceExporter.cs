using System;
using System.IO;
using AutoMapper;
using Gerakul.FastSql.Common;
using Gerakul.FastSql.SqlServer;
using Google.Protobuf;
using ReportService.Interfaces.Core;
using ReportService.Interfaces.ReportTask;

namespace ReportService.Operations.DataExporters
{
    public class ReportInstanceExporter : CommonDataExporter
    {
        public string ConnectionString;
        public string TableName;
        public int DbTimeOut;
        private readonly IArchiver archiver;
        public string ReportName;

        public ReportInstanceExporter(IMapper mapper, IArchiver archiver,
                                      ReportInstanceExporterConfig config)
        {
            this.archiver = archiver;
            mapper.Map(config, this);
        }

        public override void Send(IRTaskRunContext taskContext)
        {
            var package = taskContext.Packages[PackageName];

            if (!RunIfVoidPackage && package.DataSets.Count == 0)
                return;
            var context = SqlContextProvider.DefaultInstance
                .CreateContext(ConnectionString);

            context.CreateSimple($@"
                IF OBJECT_ID('{TableName}') IS NULL
                CREATE TABLE {TableName}
                (Id INT IDENTITY,
                ReportName NVARCHAR(255) NOT NULL,
                ExecuteTime DATETIME NOT NULL,
                OperationPackage VARBINARY(MAX) NOT NULL,
                CONSTRAINT [PK_Report_Date] PRIMARY KEY CLUSTERED 
                (ReportName DESC,
              	ExecuteTime DESC));")
                .ExecuteNonQuery();

            byte[] archivedPackage;

            using (var stream = new MemoryStream())
            {
                package.WriteTo(stream);
                archivedPackage = archiver.CompressStream(stream);
            }

            var newInstance = new
            {
                ReportName,
                ExecuteTime = DateTime.Now,
                OperationPackage = archivedPackage 
            };

            context.Insert(TableName, newInstance, new QueryOptions(DbTimeOut), "Id");
        }
    }
}