using System;
using Gerakul.FastSql;
using Newtonsoft.Json;
using ReportService.Interfaces;
using Telegram.Bot.Types.Enums;

namespace ReportService.DataExporters
{
    public class DbExporter : CommonDataExporter
    {
        private readonly string name;
        private readonly string apiPath;
        private readonly string connectionString;

        public DbExporter(string jsonConfig)
        {
            var config = JsonConvert
                .DeserializeObject<DbExporterConfig>(jsonConfig);

            DataTypes = config.DataTypes;

            name = config.Name;
            apiPath = config.ApiPath;
            connectionString = config.ConnectionString;
        }

        public override void Send(SendData sendData)
        {
            if (!string.IsNullOrEmpty(connectionString))
            {
                try
                {
                    MappedCommand.InsertAndGetId(connectionString, "RecepientGroup", sendData.JsonBaseData, "Id");
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    throw;
                }
            }
        }
    }
}
