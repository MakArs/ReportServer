using System.Collections.Generic;
using System.Data;
using System.Text.RegularExpressions;
using AutoMapper;
using Gerakul.FastSql.Common;
using Gerakul.FastSql.SqlServer;
using ReportService.Interfaces.Core;
using ReportService.Interfaces.Protobuf;
using ReportService.Interfaces.ReportTask;

namespace ReportService.Operations.DataImporters
{
    public class DbImporter : IDataImporter
    {
        private readonly IPackageBuilder packageBuilder;

        public int Id { get; set; }
        public bool IsDefault { get; set; }
        public int Number { get; set; }
        public string Name { get; set; }
        public string PackageName { get; set; }
        public string ConnectionString;
        public string Query;
        public int TimeOut;
        private readonly List<object> values;
        private bool parametersSet;

        public DbImporter(IMapper mapper, DbImporterConfig config, IPackageBuilder builder)
        {
            mapper.Map(config, this);
            packageBuilder = builder;
            values = new List<object>();
        }

        private void SetParameters(IRTaskRunContext taskContext)
        {
            Regex paramName = new Regex(@"\@RepPar\w+\b");

            var selections = paramName.Matches(Query);

            for (int i = 0; i < selections?.Count; i++)
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
                            taskContext.Packages[PackageName] = pack;
                        });

                else
                    connectionContext
                        .CreateSimple(new QueryOptions(TimeOut), $"{Query}")
                        .UseReader(reader =>
                        {
                            var pack = packageBuilder.GetPackage(reader);
                            taskContext.Packages[PackageName] = pack;
                        });
            });
        }
    }
}