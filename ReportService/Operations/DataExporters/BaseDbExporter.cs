using AutoMapper;
using Google.Protobuf.Collections;
using ReportService.Entities;
using ReportService.Interfaces.Operations;
using ReportService.Interfaces.Protobuf;
using ReportService.Interfaces.ReportTask;
using ReportService.Operations.DataExporters.Configurations;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReportService.Operations.DataExporters
{
    public abstract class BaseDbExporter : IOperation
    {
        public CommonOperationProperties Properties { get; set; } = new CommonOperationProperties();
        public bool RunIfVoidPackage { get; set; }

        protected readonly IPackageParser packageParser;
        protected Dictionary<ScalarType, string> scalarTypesToSqlTypes;

        public string ConnectionString;
        public string TableName;
        public int DbTimeOut;
        public bool DropBefore;
        public bool CreateTable;

        public BaseDbExporter(IMapper mapper, DbExporterConfig config, IPackageParser parser)
        {
            mapper.Map(config, this);
            mapper.Map(config, Properties);
            packageParser = parser;
        }

        protected virtual string BuildInsertQuery(RepeatedField<ColumnInfo> columns)
        {
            StringBuilder query = new StringBuilder($@"INSERT INTO ""{TableName}"" (");
            for (int i = 0; i < columns.Count - 1; i++)
            {
                query.Append($@"""{columns[i].Name}"",");
            }

            query.Append($@"""{columns.Last().Name}"") VALUES (");

            int j;
            for (j = 0; j < columns.Count - 1; j++)
            {
                query.Append($"@p{j},");
            }

            query.Append($"@p{j})");

            return query.ToString();
        }

        public abstract Task ExecuteAsync(IReportTaskRunContext taskContext);
        protected abstract string BuildCreateTableQuery(RepeatedField<ColumnInfo> columns);
    }
}