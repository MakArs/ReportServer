using System.Collections.Generic;
using System.Linq;
using System.Text;
using Gerakul.FastSql;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace ReportService.DataExporters
{
    public class DbExporter : CommonDataExporter
    {
        private readonly string connectionString;
        private readonly string tableName;
        private readonly int dbTimeOut;
        private readonly bool dropBefore;

        public DbExporter(string jsonConfig)
        {
            var config = JsonConvert
                .DeserializeObject<DbExporterConfig>(jsonConfig);

            Number = config.Number;
            connectionString = config.ConnectionString;
            DataSetName = config.DataSetName;
            tableName = config.TableName;
            dbTimeOut = config.DbTimeOut;
            dropBefore = config.DropBefore;
        }

        public override void Send(string dataSet)
        {
            if (!dropBefore)
                SimpleCommand.ExecuteNonQuery(new QueryOptions(dbTimeOut), 
                    connectionString, $"delete {tableName}");

            var children = JsonConvert.DeserializeObject<List<Dictionary<string, object>>>(dataSet);

            var names = children.First().Select(pair => pair.Key).ToList();

            StringBuilder comm = new StringBuilder($"INSERT INTO {tableName} (");
            for (int i = 0; i < names.Count-1; i++)
            {
                comm.Append($"[{names[i]}],");
            }

            comm.Append($"[{names.Last()}]) VALUES (");

            foreach (var child in children)
            {
                var values= child.Select(pair => pair.Value).ToList();
                var fullcom=new StringBuilder(comm.ToString());
                for (int i = 0; i < names.Count - 1; i++)
                {
                    fullcom.Append($"'{values[i]}',");
                }
                fullcom.Append($"'{values.Last()}')");
                SimpleCommand.ExecuteNonQuery( new QueryOptions(dbTimeOut), connectionString, fullcom.ToString());
            }
        }
    }
}
