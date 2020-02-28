using AutoMapper;
using Dapper;
using Google.Protobuf.Collections;
using Npgsql;
using ReportService.Interfaces.Protobuf;
using ReportService.Interfaces.ReportTask;
using ReportService.Operations.DataExporters.Configurations;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReportService.Operations.DataExporters
{
    public class PostgresDbExporter : BaseDbExporter
    {
        protected override Dictionary<ScalarType, string> ScalarTypesToSqlTypes { get; set; }

        public PostgresDbExporter(IMapper mapper, DbExporterConfig config, IPackageParser parser) :
            base(mapper, config, parser)
        {
            ScalarTypesToSqlTypes =
                new Dictionary<ScalarType, string>
                {
                    {ScalarType.Int32, "INT"},
                    {ScalarType.Double, "DOUBLE PRECISION"},
                    {ScalarType.Int64, "BIGINT"},
                    {ScalarType.Bool, "BOOLEAN"},
                    {ScalarType.String, "VARCHAR(4000)"},
                    {ScalarType.Bytes, "BYTEA"},
                    {ScalarType.DateTime, "TIMESTAMP(3)"},
                    {ScalarType.Int16, "SMALLINT"},
                    {ScalarType.Int8, "SMALLINT"},
                    {ScalarType.DateTimeOffset, "TIMESTAMP(3) WITH TIME ZONE"},
                    {ScalarType.TimeSpan, "TIME"},
                    {ScalarType.Decimal, "NUMERIC"}
                    // {ScalarType.TimeStamp, typeof(DateTime)}
                };
        }

        protected override string BuildCreateTableQuery(RepeatedField<ColumnInfo> columns)
        {
            StringBuilder createQueryBuilder = new StringBuilder($@"CREATE TABLE IF NOT EXISTS 
                ""{TableName}""(");

            var i = 1;

            foreach (var col in columns)
            {
                var nullable = col.Nullable ? "NULL" : "NOT NULL";
                createQueryBuilder.AppendLine(@$"""{col.Name ?? $"NoNameColumn{i++}"}"" {ScalarTypesToSqlTypes[col.Type]} {nullable},");
            }

            var index = createQueryBuilder.ToString().LastIndexOf(',');
            if (index >= 0)
                createQueryBuilder.Remove(index, 1);

            createQueryBuilder.Append(");");

            return createQueryBuilder.ToString();
        }

        public override async Task ExecuteAsync(IReportTaskRunContext taskContext)
        {
            var token = taskContext.CancelSource.Token;

            await using var connection = new NpgsqlConnection(ConnectionString);

            var package = taskContext.Packages[Properties.PackageName];

            if (!RunIfVoidPackage && package.DataSets.Count == 0)
                return;

            var firstSet = packageParser.GetPackageValues(package).First();

            var columns = package.DataSets.First().Columns;

            if (CreateTable)
                await connection.ExecuteAsync(new CommandDefinition(BuildCreateTableQuery(columns),
                    commandTimeout: DbTimeOut,
                    cancellationToken: token)); //todo:logic for auto-creating table by user-defined list of columns?

            if (DropBefore)
                await connection.ExecuteAsync(new CommandDefinition(
                    $@"DO
                        $$
                        BEGIN
	                        IF(SELECT EXISTS(
			                        SELECT 1 
			                        FROM pg_tables
			                        WHERE tablename = '{TableName}'
		                        ))THEN
		                        DELETE FROM ""{TableName}"";
                            END IF;
                        END;
                        $$",
                    commandTimeout: DbTimeOut, cancellationToken: token));

            var query = BuildInsertQuery(columns);

            var dynamicRows = firstSet.Rows.Select(row =>
            {
                var p = new DynamicParameters();

                for (int i = 0; i < row.Count; i++)
                {
                    p.Add($"@p{i}", row[i]);
                }

                return p;
            });

            await connection.ExecuteAsync(new CommandDefinition(
                query, dynamicRows,
                commandTimeout: DbTimeOut, cancellationToken: token));
        }
    }
}
