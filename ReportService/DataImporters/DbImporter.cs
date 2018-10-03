using System.Collections.Generic;
using AutoMapper;
using Gerakul.FastSql.Common;
using Gerakul.FastSql.SqlServer;
using Newtonsoft.Json;
using ReportService.Interfaces;

namespace ReportService.DataImporters
{
    public class DbImporter : IDataImporter
    {
        public int Id { get; set; }
        public bool IsDefault { get; set; }
        public int Number { get; set; }
        public string Name { get; set; }
        public string DataSetName { get; set; }
        public string ConnectionString;
        public string Query;
        public int TimeOut;

        public DbImporter(IMapper mapper, DbImporterConfig config)
        {
            mapper.Map(config, this);
        }

        public string Execute()
        {
            var queryResult = new List<Dictionary<string, object>>();
            // var queryres2=new Dictionary<string,List<object>>();

            var context = SqlContextProvider.DefaultInstance
                .CreateContext(ConnectionString);
            context.UsingConnection(connectionContext =>
            {
                var opt = new QueryOptions(TimeOut);
                connectionContext
                    .CreateSimple(opt, $"{Query}")
                    .UseReader(reader =>
                    {
                        if (reader.Read())
                        {
                            var fields = new Dictionary<string, object>();

                            for (int i = 0; i < reader.FieldCount; i++)
                            {
                                var name = reader.GetName(i);
                                var val = reader[i];
                                // queryres2[name] = new List<object> {val};
                                fields.Add(name, val);
                            }

                            queryResult.Add(fields);
                        }

                        while (reader.Read())
                        {
                            var fields = new Dictionary<string, object>();

                            for (int i = 0; i < reader.FieldCount; i++)
                            {
                                var name = reader.GetName(i);
                                var val = reader[i];
                                //  queryres2[name].Add(val);
                                fields.Add(name, val);
                            }

                            queryResult.Add(fields);
                        }
                    });
            });

            string jsString = JsonConvert.SerializeObject(queryResult);
            // string jsString = JsonConvert.SerializeObject(queryres2,Formatting.Indented);
            return jsString;
        }
    }
}