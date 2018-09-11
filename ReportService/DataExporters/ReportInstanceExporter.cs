using System;
using Gerakul.FastSql;
using Newtonsoft.Json;

namespace ReportService.DataExporters
{
    public class ReportInstanceExporter : CommonDataExporter
    {
        private readonly string connectionString;
        private readonly string tableName;
        private readonly int dbTimeOut;
        private readonly string reportName;

        public ReportInstanceExporter(string jsonConfig)
        {
            var config = JsonConvert
                .DeserializeObject<ReportInstanceExporterConfig>(jsonConfig);

            reportName = config.ReportName;
            connectionString = config.ConnectionString;
            DataSetName = config.DataSetName;
            tableName = config.TableName;
            dbTimeOut = config.DbTimeOut;
        }

        public override void Send(string dataSet)
        {

            SimpleCommand.ExecuteNonQuery(connectionString, $@"
                IF OBJECT_ID('{tableName}') IS NULL
                CREATE TABLE {tableName}
                (Id INT IDENTITY,
                ReportName NVARCHAR(255) NOT NULL,
                ExecuteTime DATETIME NOT NULL,
                Data NVARCHAR(MAX) NOT NULL,
                CONSTRAINT [PK_Report_Date] PRIMARY KEY CLUSTERED 
                (ReportName DESC,
              	ExecuteTime DESC));");

            var newInstance = new
            {
                ReportName = reportName,
                ExecuteTime = DateTime.Now,
                Data = dataSet
            };

            MappedCommand.Insert(new QueryOptions(dbTimeOut), connectionString,
                tableName, newInstance, "Id");
        }
    }
}
