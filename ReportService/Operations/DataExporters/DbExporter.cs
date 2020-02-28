using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using Dapper;
using Google.Protobuf.Collections;
using ReportService.Interfaces.Protobuf;
using ReportService.Interfaces.ReportTask;
using ReportService.Operations.DataExporters.Configurations;

namespace ReportService.Operations.DataExporters
{
    public class DbExporter : BaseDbExporter
    {
        protected override Dictionary<ScalarType, string> ScalarTypesToSqlTypes { get; set; }
        protected override string CleanTableQuery =>
            $"IF OBJECT_ID('{TableName}') IS NOT NULL DELETE {TableName}";

        public DbExporter(IMapper mapper, DbExporterConfig config, IPackageParser parser) :
            base(mapper, config, parser)
        {
            ScalarTypesToSqlTypes =
                new Dictionary<ScalarType, string>
                {
                    {ScalarType.Int32, "INT"},
                    {ScalarType.Double, "FLOAT"},
                    {ScalarType.Int64, "BIGINT"},
                    {ScalarType.Bool, "BIT"},
                    {ScalarType.String, "NVARCHAR(4000)"},
                    {ScalarType.Bytes, "VARBINARY(MAX)"},
                    {ScalarType.DateTime, "DATETIME"},
                    {ScalarType.Int16, "SMALLINT"},
                    {ScalarType.Int8, "TINYINT"},
                    {ScalarType.DateTimeOffset, "DATETIMEOFFSET(3)"},
                    {ScalarType.TimeSpan, "TIME"},
                    {ScalarType.Decimal, "DECIMAL"}
                    // {ScalarType.TimeStamp, typeof(DateTime)}
                };
        }

        protected override string BuildCreateTableQuery(RepeatedField<ColumnInfo> columns)
        {
            StringBuilder createQueryBuilder = new StringBuilder($@"IF OBJECT_ID('{TableName}') IS NULL
                CREATE TABLE {TableName}(");

            var i = 1;

            foreach (var col in columns)
            {
                var nullable = col.Nullable ? "NULL" : "NOT NULL";
                createQueryBuilder.AppendLine(@$"[{col.Name ?? $"NoNameColumn{i++}"}] {ScalarTypesToSqlTypes[col.Type]} {nullable},");
            }

            var index = createQueryBuilder.ToString().LastIndexOf(',');
            if (index >= 0)
                createQueryBuilder.Remove(index, 1);

            createQueryBuilder.Append(");");

            return createQueryBuilder.ToString();
        }

        public override async Task ExecuteAsync(IReportTaskRunContext taskContext)
        {
            await using var connection = new SqlConnection(ConnectionString);

            await ExportDataSet(taskContext, connection);
        }
    }
}