using System.Collections.Generic;
using System.Data;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using AutoMapper;
using Gerakul.FastSql.Common;
using Gerakul.FastSql.SqlServer;
using ReportService.Interfaces.Core;
using ReportService.Interfaces.Protobuf;
using ReportService.Interfaces.ReportTask;

namespace ReportService.Operations.DataImporters
{
    public class DbImporter : IOperation
    {
        public CommonOperationProperties Properties { get; set; } = new CommonOperationProperties();

        private readonly IPackageBuilder packageBuilder;
        private bool parametersSet;
        private readonly List<object> values;

        public string ConnectionString;
        public string Query;
        public int TimeOut;

        public DbImporter(IMapper mapper, DbImporterConfig config, IPackageBuilder builder)
        {
            mapper.Map(config, this);
            mapper.Map(config, Properties);
            packageBuilder = builder;
            values = new List<object>();
        }

        private void SetParameters(IRTaskRunContext taskContext)
        {
            Regex paramName = new Regex(@"\@RepPar\w+\b");

            var selections = paramName.Matches(Query);

            for (int i = 0; i < selections.Count; i++)
            {
                var sel = selections[i].Value;

                if (!taskContext.Parameters.ContainsKey(sel))
                    throw new DataException($"There is no parameter {sel} in the task");

                var paramValue = taskContext.Parameters[sel];
                values.Add(paramValue);
                Query = Query.Replace(sel, $"@p{i}");
            }

            parametersSet = true;
        }

        public void Execute(IRTaskRunContext taskContext)
        {
            var sqlContext = SqlContextProvider.DefaultInstance
                .CreateContext(ConnectionString);

            if (!parametersSet)
                SetParameters(taskContext);

            sqlContext.UsingConnection(connectionContext =>
            {
                if (values.Count > 0)
                    connectionContext
                        .CreateSimple(new QueryOptions(TimeOut), $"{Query}",
                            values.ToArray())
                        .UseReader(reader =>
                        {
                            var pack = packageBuilder.GetPackage(reader);
                            taskContext.Packages[Properties.PackageName] = pack;
                        });

                else
                    connectionContext
                        .CreateSimple(new QueryOptions(TimeOut), $"{Query}")
                        .UseReader(reader =>
                        {
                            var pack = packageBuilder.GetPackage(reader);
                            taskContext.Packages[Properties.PackageName] = pack;
                        });
            });
        }

        public async Task ExecuteAsync(IRTaskRunContext taskContext)
        {
            var sqlContext = SqlContextProvider.DefaultInstance
                .CreateContext(ConnectionString);

            if (!parametersSet)
                SetParameters(taskContext);

            await sqlContext.UsingConnectionAsync(async connectionContext =>
            {
                if (values.Count > 0)
                    await connectionContext
                        .CreateSimple(new QueryOptions(TimeOut), $"{Query}",
                            values.ToArray())
                        .UseReaderAsync(reader =>
                        {
                            var pack = packageBuilder.GetPackage(reader);
                            taskContext.Packages[Properties.PackageName] = pack;
                            return Task.CompletedTask;
                        });

                else
                    await connectionContext
                        .CreateSimple(new QueryOptions(TimeOut), $"{Query}")
                        .UseReaderAsync(reader =>
                        {
                            var pack = packageBuilder.GetPackage(reader);
                            taskContext.Packages[Properties.PackageName] = pack;
                            return Task.CompletedTask;
                        });
            });
        }
    }
}