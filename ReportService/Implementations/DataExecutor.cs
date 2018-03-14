using System.Collections.Generic;
using Gerakul.FastSql;
using Newtonsoft.Json;
using ReportService.Interfaces;

namespace ReportService.Implementations
{
    public class DataExecutorTest : IDataExecutor
    {
        private string connStr = @"Data Source=WS-00005; Initial Catalog=ReportBase; Integrated Security=True"; // TODO: change connstring

        public DataExecutorTest()
        { }

        public string Execute(string aquery, int aTimeOut)
        {
            var queryResult = new List<Dictionary<string, object>>();

            SqlScope.UsingConnection(connStr, scope =>
                {
                    using (var reader = scope.CreateSimple(new QueryOptions(aTimeOut), $"{aquery}").ExecuteReader())
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

    public class DataExecutorSunday : IDataExecutor
    {
        public string Execute(string aquery, int aTimeOut)
        {
            var queryResult = new List<Dictionary<string, object>>();
            queryResult.Add(new Dictionary<string, object>());

            string jsString = JsonConvert.SerializeObject(queryResult);
            return jsString;
        }
    }
}