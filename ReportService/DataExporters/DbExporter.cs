using Gerakul.FastSql;
using Newtonsoft.Json;

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

            connectionString = config.ConnectionString;
            DataSetName = config.DataSetName;
            tableName = config.TableName;
            dbTimeOut = config.DbTimeOut;
            dropBefore = config.DropBefore;
        }

        public override void Send(string dataSet)
        {
            if (dropBefore)
                SimpleCommand.ExecuteNonQuery(new QueryOptions(dbTimeOut), 
                    connectionString, $"delete {tableName}");

            string[] someArray = {"dsa", "hksr"}; //todo:parse dataset to table logics here or somwhere else?
            someArray.WriteToServer(connectionString, "das");
        }
    }
}
