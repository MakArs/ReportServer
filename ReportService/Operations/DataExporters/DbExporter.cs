using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using Gerakul.FastSql.Common;
using Gerakul.FastSql.SqlServer;
using Google.Protobuf.Collections;
using ReportService.Entities;
using ReportService.Interfaces.Operations;
using ReportService.Interfaces.Protobuf;
using ReportService.Interfaces.ReportTask;
using ReportService.Operations.DataExporters.Configurations;

namespace ReportService.Operations.DataExporters
{
    public class DbExporter : IOperation
    {
        public CommonOperationProperties Properties { get; set; } = new CommonOperationProperties();
        public bool RunIfVoidPackage { get; set; }

        private readonly IPackageParser packageParser;
        private readonly Dictionary<ScalarType, string> scalarTypesToSqlTypes;

        public string ConnectionString;
        public string TableName;
        public int DbTimeOut;
        public bool DropBefore;
        public bool CreateTable;

        public DbExporter(IMapper mapper, DbExporterConfig config, IPackageParser parser)
        {
            mapper.Map(config, this);
            mapper.Map(config, Properties);
            packageParser = parser;

            scalarTypesToSqlTypes =
                new Dictionary<ScalarType, string>
                {
                    {ScalarType.Int32, "int"},
                    {ScalarType.Double, "float"},
                    {ScalarType.Int64, "bigint"},
                    {ScalarType.Bool, "bit"},
                    {ScalarType.String, "nvarchar(4000)"},
                    {ScalarType.Bytes, "varbinary(MAX)"},
                    {ScalarType.DateTime, "datetime"},
                    {ScalarType.Int16, "smallint"},
                    {ScalarType.Int8, "tinyint"},
                    {ScalarType.DateTimeOffset, "datetimeoffset"},
                    {ScalarType.TimeSpan, "time"},
                    {ScalarType.Decimal, "decimal"}
                    // {ScalarType.TimeStamp, typeof(DateTime)}
                };
        }

        private string CreateTableByColumnInfo(RepeatedField<ColumnInfo> columns)
        {
            StringBuilder createQueryBuilder = new StringBuilder($@"
                IF OBJECT_ID('{TableName}') IS NULL
                CREATE TABLE {TableName}
                (");

            foreach (var col in columns)
            {
                var nullable = col.Nullable ? "NULL" : "NOT NULL";
                createQueryBuilder.AppendLine($"[{col.Name}] {scalarTypesToSqlTypes[col.Type]} " +
                                              $"{nullable},");
            }

            createQueryBuilder.Length--;
            createQueryBuilder.Append("); ");

            return createQueryBuilder.ToString();
        }

        public async Task ExecuteAsync(IReportTaskRunContext taskContext)
        {
            var sqlContext = SqlContextProvider.DefaultInstance
                .CreateContext(ConnectionString);

            var token = taskContext.CancelSource.Token;

            await sqlContext.UsingConnectionAsync(async connectionContext =>
            {
                var package = taskContext.Packages[Properties.PackageName];

                if (!RunIfVoidPackage && package.DataSets.Count == 0)
                    return;

                var firstSet = packageParser.GetPackageValues(package).First();

                var columns = package.DataSets.First().Columns;

                if (CreateTable)
                    await connectionContext.CreateSimple(new QueryOptions(DbTimeOut), CreateTableByColumnInfo(columns))
                        .ExecuteNonQueryAsync(token);

                //todo:logic for auto-creating table by user-defined list of columns?
                if (DropBefore)
                    await connectionContext.CreateSimple(new QueryOptions(DbTimeOut),
                            $"IF OBJECT_ID('{TableName}') IS NOT NULL DELETE {TableName}")
                        .ExecuteNonQueryAsync(token);

                StringBuilder comm = new StringBuilder($@"INSERT INTO {TableName} (");
                for (int i = 0; i < columns.Count - 1; i++)
                {
                    comm.Append($@"[{columns[i].Name}],");
                }

                comm.Append($@"[{columns.Last().Name}]) VALUES (");

                foreach (var row in firstSet.Rows)
                {

                    var fullRowData = new StringBuilder(comm.ToString());
                    int i;

                    for (i = 0; i < columns.Count - 1; i++)
                    {
                        fullRowData.Append($"@p{i},");
                    }

                    fullRowData.Append($"@p{i})");

                    await connectionContext
                        .CreateSimple(new QueryOptions(DbTimeOut), fullRowData.ToString(), row.ToArray())
                        .ExecuteNonQueryAsync(token);
                }
            });
        }
    }
}