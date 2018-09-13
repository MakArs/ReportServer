using System.Collections.Generic;
using System.Linq;
using System.Text;
using AutoMapper;
using Gerakul.FastSql;
using Newtonsoft.Json;

namespace ReportService.DataExporters
{
    public class DbExporter : CommonDataExporter
    {
        public string ConnectionString;
        public string TableName;
        public int DbTimeOut;
        public bool DropBefore;

        public DbExporter(IMapper mapper, DbExporterConfig config)
        {
            mapper.Map(config, this);
        }

        public override void Send(string dataSet)
        {
            if (!DropBefore)
                SimpleCommand.ExecuteNonQuery(new QueryOptions(DbTimeOut),
                    ConnectionString, $"delete {TableName}");

            SimpleCommand.ExecuteNonQuery(ConnectionString, $@"
                IF OBJECT_ID('{TableName}') IS NULL
                CREATE TABLE {TableName}
                (Name NVARCHAR(4000) NOT NULL,
                Amount NVARCHAR(4000) NOT NULL); ");

            var children = JsonConvert.DeserializeObject<List<Dictionary<string, object>>>(dataSet);

            var names = children.First().Select(pair => pair.Key).ToList();

            StringBuilder comm = new StringBuilder($"INSERT INTO {TableName} (");
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

                SimpleCommand.ExecuteNonQuery( new QueryOptions(DbTimeOut), ConnectionString, fullcom.ToString());
            }
        }
    }
}
