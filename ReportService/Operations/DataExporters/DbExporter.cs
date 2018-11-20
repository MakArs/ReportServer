using System.Collections.Generic;
using System.Linq;
using System.Text;
using AutoMapper;
using Gerakul.FastSql.Common;
using Gerakul.FastSql.SqlServer;
using Google.Protobuf.Collections;
using ReportService.Interfaces.Protobuf;
using ReportService.Interfaces.ReportTask;

namespace ReportService.Operations.DataExporters
{
    public class DbExporter : CommonDataExporter
    {
        private readonly IPackageBuilder packageBuilder;
        private readonly Dictionary<ScalarType, string> scalarTypesToSqlTypes;

        public string ConnectionString;
        public string TableName;
        public int DbTimeOut;
        public bool DropBefore;
        public bool CreateTable;

        public DbExporter(IMapper mapper, DbExporterConfig config, IPackageBuilder builder)
        {
            mapper.Map(config, this);
            packageBuilder = builder;

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

        private void CreateTableByColumnInfo(DbContext sqlContext, RepeatedField<ColumnInfo> columns)
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

            sqlContext.CreateSimple(new QueryOptions(DbTimeOut), createQueryBuilder.ToString())
                .ExecuteNonQuery();
        }

        public override void Send(IRTaskRunContext taskContext)
        {
            var sqlContext = SqlContextProvider.DefaultInstance
                .CreateContext(ConnectionString);

            var package = taskContext.Packages[PackageName];

            if (!RunIfVoidPackage && package.DataSets.Count == 0)
                return;

            var firstSet = packageBuilder.GetPackageValues(package).First();

            //todo:logic for auto-creating table by user-defined list of columns
            if (DropBefore)
                sqlContext.CreateSimple(new QueryOptions(DbTimeOut),
                        $"IF OBJECT_ID('{TableName}') IS NOT NULL DELETE {TableName}")
                    .ExecuteNonQuery();

            var columns = package.DataSets.First().Columns;

            if (CreateTable)
                CreateTableByColumnInfo(sqlContext, columns);

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

                //var parameters = row.ToArray();

                sqlContext.CreateSimple(new QueryOptions(DbTimeOut), fullRowData.ToString(), row.ToArray())
                    .ExecuteNonQuery();
            }
        }
    }
}