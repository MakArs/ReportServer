using System.Collections.Generic;
using Google.Protobuf.Collections;
using ReportService.Interfaces.Protobuf;

namespace ReportService.Operations.Helpers
{
    public class DbPackageExportScriptCreator : BasePackageExportScriptCreator
    {
        protected override Dictionary<ScalarType, string> ScalarTypesToSqlTypes { get; set; }
        public DbPackageExportScriptCreator(IPackageParser packageParser) : base(packageParser)
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

        protected override void InitializeCreateTableQuery(RepeatedField<ColumnInfo> columns, string temporaryTableName, SqlCommandInitializer commandBuilder)
        {
            commandBuilder.AppendQueryString($@"CREATE TABLE {temporaryTableName} (");
            
            var i = 1;
            foreach (var col in columns)
            {
                var nullable = col.Nullable ? "NULL" : "NOT NULL";
                commandBuilder.AppendQueryString(@$"[{col.Name ?? $"NoNameColumn{i++}"}] {ScalarTypesToSqlTypes[col.Type]} {nullable},");
            }
            commandBuilder.HandleClosingBracket();
        }
    }
}