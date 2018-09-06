using System.Collections.Generic;
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
        private readonly string connectionString;
        private readonly string query;
        private readonly int timeOut;

        public DbImporter(string jsonConfig)
        {
            var dbConfig = JsonConvert
                .DeserializeObject<DbImporterConfig>(jsonConfig);

            Number = dbConfig.Number;
            DataSetName = dbConfig.DataSetName;
            connectionString = dbConfig.ConnectionString;
            query = dbConfig.Query;
            timeOut = dbConfig.TimeOut;
        }

        public string Execute()
        {
            var queryResult = new List<Dictionary<string, object>>();

            SqlScope.UsingConnection(connectionString, scope =>
            {
                using (var reader = scope
                    .CreateSimple(new QueryOptions(timeOut), $"{query}")
                    .ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var fields = new Dictionary<string, object>();

                        for (int i = 0; i < reader.FieldCount; i++)
                            fields.Add(reader.GetName(i), reader[i]);

                        queryResult.Add(fields);
                    }
                }
            });

            string jsString = JsonConvert.SerializeObject(queryResult);
            return jsString;
        }
    }
}