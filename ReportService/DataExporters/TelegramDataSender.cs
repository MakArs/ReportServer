using System;
using Autofac;
using AutoMapper;
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
        public string ReportName;

        public TelegramDataSender(IMapper mapper, ITelegramBotClient botClient, ILifetimeScope autofac,
                                  ILogic logic, TelegramExporterConfig config)
        {
            mapper.Map(config, this);

            channel = logic.GetTelegramChatIdByChannelId(config.TelegramChannelId);
            viewExecutor = autofac.ResolveNamed<IViewExecutor>("commonviewex");
            bot = botClient;
        }

        public override void Send(string dataSet)
        {
                bot.SendTextMessageAsync(channel.ChatId,
                        viewExecutor.ExecuteTelegramView(dataSet, ReportName),
                        ParseMode.Markdown)
                    .Wait();
        }
    }
}