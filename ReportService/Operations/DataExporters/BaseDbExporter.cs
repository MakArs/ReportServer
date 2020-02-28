using AutoMapper;
using Dapper;
using Google.Protobuf.Collections;
using ReportService.Entities;
using ReportService.Interfaces.Operations;
using ReportService.Interfaces.Protobuf;
using ReportService.Interfaces.ReportTask;
using ReportService.Operations.DataExporters.Configurations;
using System;
using System.Collections.Generic;
using System.Data.Common;
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
        protected abstract Dictionary<ScalarType, string> ScalarTypesToSqlTypes { get; set; }
        protected abstract string CleanTableQuery { get; }

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

        protected virtual async Task ExportDataSet(IReportTaskRunContext taskContext, DbConnection connection)
        {
            var token = taskContext.CancelSource.Token;

            var package = taskContext.Packages[Properties.PackageName];

            if (!RunIfVoidPackage && package.DataSets.Count == 0)
                return;

            var firstSet = packageParser.GetPackageValues(package).First();

            var columns = package.DataSets.First().Columns;

            if (CreateTable)
                await connection.ExecuteAsync(new CommandDefinition(BuildCreateTableQuery(columns),
                    commandTimeout: DbTimeOut,
                    cancellationToken: token)); //todo:logic for auto-creating table by user-defined list of columns?
            
            connection.Open();
            await using var transaction= connection.BeginTransaction();
            
            try 
            {
                if (DropBefore)
                    await connection.ExecuteAsync(new CommandDefinition(CleanTableQuery,
                        commandTimeout: DbTimeOut, cancellationToken: token, transaction: transaction));

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
                    commandTimeout: DbTimeOut, cancellationToken: token, 
                    transaction: transaction));

                transaction.Commit();
            }

            catch (Exception e)
            {
                transaction.Rollback();
                throw;
            }
        }

        public abstract Task ExecuteAsync(IReportTaskRunContext taskContext);

        protected abstract string BuildCreateTableQuery(RepeatedField<ColumnInfo> columns);
    }
}