using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AutoMapper;
using Gerakul.FastSql.Common;
using Gerakul.FastSql.SqlServer;
using ReportService.Interfaces.Protobuf;
using ReportService.Interfaces.ReportTask;

namespace ReportService.Operations.DataExporters
{
    public class DbExporter : CommonDataExporter
    {
        private readonly IPackageBuilder packageBuilder;
        private readonly Dictionary<ScalarType, string> ScalarTypesToSqlTypes;

        public string ConnectionString;
        public string TableName;
        public int DbTimeOut;
        public bool DropBefore;
        public bool CreateTable;

        public DbExporter(IMapper mapper, DbExporterConfig config, IPackageBuilder builder)
        {
            mapper.Map(config, this);
            packageBuilder = builder;

            ScalarTypesToSqlTypes =
                new Dictionary<ScalarType, string>
                {
                    {ScalarType.Int32, "int"},
                    {ScalarType.Double, "float"},
                    {ScalarType.Int64, "bigint"},
                    {ScalarType.Bool, "bit"},
                    {ScalarType.String, "nvarchar(4000)"},
                    {ScalarType.Bytes, "varbinary(MAX)"},
                    {ScalarType.DateTime, "datetime"},
                    // {ScalarType.TimeStamp, typeof(DateTime)}
                };
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
            {
                StringBuilder createQueryBuilder = new StringBuilder($@" 
                IF OBJECT_ID('{TableName}') IS NULL
                CREATE TABLE {TableName}
                (");

                foreach (var col in columns)
                {
                    var nullable = col.Nullable ? "NULL" : "NOT NULL";
                    createQueryBuilder.AppendLine($"{col.Name} {ScalarTypesToSqlTypes[col.Type]} " +
                                                  $"{nullable},");
                }

                createQueryBuilder.Length--;
                createQueryBuilder.Append("); ");

                sqlContext.CreateSimple(createQueryBuilder.ToString())
                    .ExecuteNonQuery();
            }

            StringBuilder comm = new StringBuilder($@"INSERT INTO {TableName} (");
            for (int i = 0; i < columns.Count - 1; i++)
            {
                comm.Append($@"[{columns[i].Name}],");
            }

            comm.Append($@"[{columns.Last().Name}]) VALUES (");

            foreach (var row in firstSet.Rows)
            {

                var fullcom = new StringBuilder(comm.ToString());
                int i;

                for (i = 0; i < columns.Count - 1; i++)
                {
                    fullcom.Append($@"@p{i},");

                }

                fullcom.Append($@"@p{i})");
                var fstr = fullcom.ToString();

                var parameters = row.ToArray();

                sqlContext.CreateSimple(new QueryOptions(DbTimeOut), fstr, parameters)
                    .ExecuteNonQuery();
            }
        }
    }
}