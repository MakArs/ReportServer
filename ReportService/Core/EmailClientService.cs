using MailKit;
using MailKit.Net.Imap;
using MailKit.Search;
using MimeKit;
using ReportService.Entities;
using ReportService.Interfaces.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ReportService.Core
{
    public class EmailClientService: IEmailClientService
    {
        private readonly ImapClient imapClient = new ImapClient();

        public async Task<MimePart> GetFileFromEmail(EmailSettings settings, CancellationToken token)
        {
            await ConnectAsync(settings, token);

            var messages = await GetAllNewMessagesAsync(settings, token);

            var msg = messages.OrderByDescending(msg => msg.Message.Date)
               .Where(msg => msg.Message.Attachments.Any
               (att => (att as MimePart)
                   .FileName == settings.AttachmentName))
               .First();

            var att = (msg.Message.Attachments.First(att => (att as MimePart)
                   .FileName == settings.AttachmentName) as MimePart);

            await DeleteMessageAsync(msg.Uid, token);

            await DisconnectAsync(CancellationToken.None);

            return att;
        }

        private async Task<List<EmailMessage>> GetAllNewMessagesAsync(EmailSettings settings, CancellationToken token)
        {
            if (!imapClient.Inbox.IsOpen)
            {
                await imapClient.Inbox.OpenAsync(FolderAccess.ReadWrite, token);
            }

            var uids = (await imapClient.Inbox.SearchAsync
                (SearchQuery.NotSeen
                .And(SearchQuery.FromContains(settings.SenderEmail)
                .And(SearchQuery.DeliveredAfter(DateTime.Today.AddDays(-settings.SearchDays)))
                ), token)).ToList();

            var messages = uids.Select(async uid =>
             {
                 var msg = await imapClient.Inbox.GetMessageAsync(uid, token);
                 return new EmailMessage { Uid = uid, Message = msg };
             }).Select(task => task.Result).ToList();

            return messages;
        }

        private async Task MarkMessageSeenAsync(UniqueId uid, CancellationToken token)
        {
            await imapClient.Inbox.AddFlagsAsync(uid, MessageFlags.Seen, true, token);
        }

        public async Task DeleteMessageAsync(UniqueId uid, CancellationToken token)
        {
            imapClient.Inbox.AddFlags(uid, MessageFlags.Deleted, true, token);
            await imapClient.Inbox.ExpungeAsync(token);
        }

        private async Task DisconnectAsync(CancellationToken token)
        {
            await imapClient.DisconnectAsync(true, token);
        }

        private async Task ConnectAsync(EmailSettings settings, CancellationToken token)
        {
            if (imapClient.IsConnected)
                return;

            await imapClient.ConnectAsync(settings.ServerHost, settings.Port,
                settings.UseImapSecureOptions
                    ? MailKit.Security.SecureSocketOptions.Auto
                    : MailKit.Security.SecureSocketOptions.None, token);

            await imapClient.AuthenticateAsync(settings.Username, settings.Password, token);
        }
    }
}