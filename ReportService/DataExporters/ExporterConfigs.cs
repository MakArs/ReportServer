using System.Collections.Generic;
using ReportService.Extensions;
using ReportService.Interfaces;

namespace ReportService.DataExporters
{
    public class DbExporterConfig : IExporterConfig
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public List<string> DataTypes { get; set; }
        public string ConnectionString { get; set; }
        public string ApiPath { get; set; }
    }

    public class EmailExporterConfig : IExporterConfig
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public List<string> DataTypes { get; set; }
        public RecepientAddresses Addresses { get; set; }
    }

    public class TelegramExporterConfig : IExporterConfig
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public List<string> DataTypes { get; set; } = new List<string>{"Telegram"};
        public string Description { get; set; }
        public long ChatId { get; set; }
        public int Type { get; set; }
    }
}