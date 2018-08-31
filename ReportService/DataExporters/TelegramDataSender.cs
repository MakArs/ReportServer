using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using ReportService.Interfaces;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;

namespace ReportService.DataExporters
{
    public class TelegramDataSender : CommonDataExporter
    {
        private readonly ITelegramBotClient bot;
        private readonly string description;
        private readonly long chatId;
        private readonly int type;
        private readonly string name;

        public TelegramDataSender(ITelegramBotClient botClient, string jsonConfig)
        {
            var config = JsonConvert
                .DeserializeObject<TelegramExporterConfig>(jsonConfig);

            name = config.Name;
            DataTypes.AddRange(config.DataTypes);
            bot = botClient;
            description = config.Description;
            chatId = config.ChatId;
            type = config.Type;
        }

        public override void Send(SendData sendData)
        {
            try
            {
                bot.SendTextMessageAsync(chatId, sendData.TelegramData, ParseMode.Markdown)
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
