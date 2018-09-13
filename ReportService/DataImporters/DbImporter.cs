using System.Collections.Generic;
using AutoMapper;
using Gerakul.FastSql;
using Newtonsoft.Json;
using ReportService.Interfaces;

namespace ReportService.DataImporters
{
    public class DbImporter : IDataImporter
    {
        public int Id { get; set; }
        public int Number { get; set; }
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
            var queryres2=new Dictionary<string,List<object>>();

            SqlScope.UsingConnection(ConnectionString, scope =>
            {
                using (var reader = scope
                    .CreateSimple(new QueryOptions(TimeOut), $"{Query}")
                    .ExecuteReader())
                {
                    if(reader.Read())
                    {
                        var fields = new Dictionary<string, object>();

                        for (int i = 0; i < reader.FieldCount; i++)
                        {
                            var name = reader.GetName(i);
                            var val = reader[i];
                            queryres2[name] = new List<object> {val};
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
                            queryres2[name].Add(val);
                            fields.Add(name, val);
                        }

                        queryResult.Add(fields);
                    }
                }
            });

            string jsString = JsonConvert.SerializeObject(queryResult);
           // string jsString = JsonConvert.SerializeObject(queryres2,Formatting.Indented);
            return jsString;
        }
    }
}