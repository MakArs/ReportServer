using System;
using AutoMapper;
using Gerakul.FastSql;
using ReportService.Interfaces;

namespace ReportService.DataExporters
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

        public override void Send(string dataSet)
        {
            if (!RunIfVoidDataSet && (string.IsNullOrEmpty(dataSet) || dataSet == "[]"))
                return;
            SimpleCommand.ExecuteNonQuery(ConnectionString, $@"
                IF OBJECT_ID('{TableName}') IS NULL
                CREATE TABLE {TableName}
                (Id INT IDENTITY,
                ReportName NVARCHAR(255) NOT NULL,
                ExecuteTime DATETIME NOT NULL,
                Data NVARCHAR(MAX) NOT NULL,
                CONSTRAINT [PK_Report_Date] PRIMARY KEY CLUSTERED 
                (ReportName DESC,
              	ExecuteTime DESC));");

            var newInstance = new
            {
                ReportName = ReportName,
                ExecuteTime = DateTime.Now,
                Data = dataSet //archiver.CompressString(dataSet)
            };

            MappedCommand.Insert(new QueryOptions(DbTimeOut), ConnectionString,
                TableName, newInstance, "Id");
        }
    }
}