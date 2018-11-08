using System.Collections.Generic;
using System.Linq;
using System.Text;
using AutoMapper;
using Gerakul.FastSql.Common;
using Gerakul.FastSql.SqlServer;
using Newtonsoft.Json;
using ReportService.Interfaces.ReportTask;

namespace ReportService.Operations.DataExporters
{
    public class DbExporter : CommonDataExporter
    {
        public string ConnectionString;
        public string TableName;
        public int DbTimeOut;
        public bool DropBefore;
        public bool CreateTable;

        public DbExporter(IMapper mapper, DbExporterConfig config)
        {
            mapper.Map(config, this);
        }

        public override void Send(IRTaskRunContext taskContext)
        {
            var sqlContext = SqlContextProvider.DefaultInstance
                .CreateContext(ConnectionString);

            var package = taskContext.Packages[PackageName];

            if (!RunIfVoidPackage && package.DataSets.Count == 0)
                return;

            //todo:logic for auto-creating table by user-defined list of columns
            if (DropBefore)
                sqlContext.CreateSimple(new QueryOptions(DbTimeOut),
                    $"IF OBJECT_ID('{TableName}') IS NOT NULL DELETE {TableName}")
                    .ExecuteNonQuery();
            var names = children.First().Select(pair => pair.Key).ToList();

            if (CreateTable)
            {
                StringBuilder createQueryBuilder = new StringBuilder($@" 
                IF OBJECT_ID('{TableName}') IS NULL
                CREATE TABLE {TableName}
                (");

                foreach (var name in names)
                {
                    createQueryBuilder.AppendLine($"{name} NVARCHAR(4000) NOT NULL,");
                }

                createQueryBuilder.Length--;
                createQueryBuilder.Append("); ");

                sqlContext.CreateSimple(createQueryBuilder.ToString())
                    .ExecuteNonQuery();
            }

            StringBuilder comm = new StringBuilder($@"INSERT INTO {TableName} (");
            for (int i = 0; i < names.Count - 1; i++)
            {
                comm.Append($@"[{names[i]}],");
            }

            comm.Append($@"[{names.Last()}]) VALUES (");

            foreach (var child in children)
            {
                var values = child.Select(pair => pair.Value).ToList();

                var fullcom = new StringBuilder(comm.ToString());

                for (int i = 0; i < names.Count - 1; i++)
                {
                    fullcom.Append($@"'{values[i]}',");
                }

                fullcom.Append($@"'{values.Last()}')");
                var fstr = fullcom.ToString();
                sqlContext.CreateSimple(new QueryOptions(DbTimeOut), fstr).ExecuteNonQuery();
            }
        }
    }
}