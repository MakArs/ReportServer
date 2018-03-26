using System.Collections.Generic;
using Gerakul.FastSql;
using Newtonsoft.Json;
using ReportService.Interfaces;

namespace ReportService.Implementations
{
    public class DataExecutor : IDataExecutor
    {
        public DataExecutor()
        {
        }

        public string Execute(RTask task)
        {
            var queryResult = new List<Dictionary<string, object>>();

            SqlScope.UsingConnection(task.ConnectionString, scope =>
            {
                using (var reader = scope.CreateSimple(new QueryOptions(task.TimeOut), $"{task.Query}").ExecuteReader())
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

        public string ExecuteNoTask(string connStr, string query, int timeOut)
        {
            var queryResult = new List<Dictionary<string, object>>();

            SqlScope.UsingConnection(connStr, scope =>
            {
                using (var reader = scope.CreateSimple(new QueryOptions(timeOut), $"{query}").ExecuteReader())
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
    } // class
}