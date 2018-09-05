using System;
using Newtonsoft.Json;
using ReportService.Interfaces;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;

namespace ReportService.DataExporters
{
    public class TelegramDataSender : CommonDataExporter
    {
        private readonly ITelegramBotClient bot;
        private readonly DtoTelegramChannel channel;
        private readonly IViewExecutor viewExecutor;
        private readonly string reportName;

        public TelegramDataSender(ITelegramBotClient botClient, IViewExecutor executor,
                                  ILogic logic, string jsonConfig)
        {
            var config = JsonConvert
                .DeserializeObject<TelegramExporterConfig>(jsonConfig);

            DataSetName = config.DataSetName;
            channel = logic.GetTelegramChatIdByChannelId(config.TelegramChannelId);
            reportName = config.ReportName;
            viewExecutor = executor;
            bot = botClient;
        }

        public override void Send(string dataSet)
        {
            try
            {
                bot.SendTextMessageAsync(channel.ChatId,
                        viewExecutor.ExecuteTelegramView(dataSet, reportName),
                        ParseMode.Markdown)
                    .Wait();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }
    }
}
