using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text.RegularExpressions;
using Google.Protobuf.Collections;
using ReportService.Interfaces.Protobuf;

namespace ReportService.Operations.DataImporters.Helpers
{
    public abstract class BasePackageExportScriptCreator
    {

        private readonly IPackageParser mPackageParser;
        private const string cReportParamKeyword = @"@RepPar";
        private readonly Regex mParamNameRegex = new Regex(@$"\B{cReportParamKeyword}\w*\b");
        protected abstract Dictionary<ScalarType, string> ScalarTypesToSqlTypes { get; set; }

        protected BasePackageExportScriptCreator(IPackageParser packageParser)
        {
            mPackageParser = packageParser;
        }

        public void BuildPackageExportQuery(OperationPackage package, string packageName, SqlCommandInitializer commandBuilder, ref int globalParamIdx)
        {
            if (package is null || package.DataSets.Count == 0)
                return;

            var tempTableName = PackageConsumerBase.ReportPackageKeyword + packageName;
            var columns = package.DataSets.First().Columns;

            InitializeCreateTableQuery(columns, tempTableName, commandBuilder);

            var firstSet = mPackageParser.GetPackageValues(package).First();

            foreach (var row in firstSet.Rows)
            {
                InitializeInsertQueryRow(columns, tempTableName, globalParamIdx, commandBuilder);

                for (int columnInRowIdx = 0; columnInRowIdx < row.Count; columnInRowIdx++)
                {
                    var value = row[columnInRowIdx];
                    commandBuilder.AddParameterWithValue($"@p{globalParamIdx++}", value);
                }
            }
        }

        protected abstract void InitializeCreateTableQuery(RepeatedField<ColumnInfo> columns, string tempTableName, SqlCommandInitializer commandBuilder);

        protected virtual void InitializeInsertQueryRow(RepeatedField<ColumnInfo> columns, string tempTableName, int globalParamIdx, SqlCommandInitializer commandBuilder)
        {
            commandBuilder.AppendQueryString($@"INSERT INTO {tempTableName} (");

            foreach (var column in columns)
            {
                commandBuilder.AppendQueryString($@"""{column.Name}"",");
            }

            commandBuilder.HandleClosingBracket();
            commandBuilder.AppendQueryString("VALUES(");

            for (int j = 0; j < columns.Count; j++)
            {
                commandBuilder.AppendQueryString($"@p{globalParamIdx + j},");
            }
            commandBuilder.HandleClosingBracket();
        }

        internal void BuildMainQuery(Dictionary<string, object> parameters, string mainQuery, SqlCommandInitializer commandInitializer, int parameterGlobalIdx)
        {
            var parametrizedString = mParamNameRegex.Replace(mainQuery, match =>
            {
                if (!parameters.ContainsKey(match.Value))
                    throw new DataException($"There is no parameter {match.Value} in the task");

                commandInitializer.AddParameterWithValue($"@p{parameterGlobalIdx++}", parameters[match.Value]);
                return match.Value.Replace(cReportParamKeyword, string.Empty);
            });

            commandInitializer.AppendQueryString(parametrizedString);
        }
    }
}
