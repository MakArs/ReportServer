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
        private readonly ImapClient mImapClient = new ImapClient();

        public async Task<MimePart> GetFileFromEmail(EmailSettings settings, CancellationToken token)
        {
            await ConnectAsync(settings, token);

            List<EmailMessage> messages = await GetNewMessagesAsync(settings, token);

            //TODO: rework. It is not an error, just normal situation
            if (messages.Count == 0)
                throw new Exception("No new emails found");

            EmailMessage message = messages.OrderByDescending(msg => msg.Message.Date)
                .FirstOrDefault(msg => msg.Message.Attachments.Any(att => (att as MimePart)?.FileName == settings.AttachmentName));

            if (message == null)
                throw new Exception("No matching files found in new emails");

            MimePart attachment = message.Message.Attachments.FirstOrDefault(att => (att as MimePart)?.FileName == settings.AttachmentName) as MimePart;

            await DeleteMessageAsync(message.Uid, token);
            await DisconnectAsync(CancellationToken.None);

            return attachment;
        }

        private async Task<List<EmailMessage>> GetNewMessagesAsync(EmailSettings settings, CancellationToken token)
        {
            if (!mImapClient.Inbox.IsOpen)
            {
                await mImapClient.Inbox.OpenAsync(FolderAccess.ReadWrite, token);
            }

            List<UniqueId> newIds = (await mImapClient.Inbox.SearchAsync(SearchQuery.NotSeen
                .And(SearchQuery.FromContains(settings.SenderEmail)
                    .And(SearchQuery.DeliveredAfter(DateTime.Today.AddDays(-settings.SearchDays)))), 
                token)).ToList();

            var messages = newIds.Select(async uid =>
             {
                 var msg = await mImapClient.Inbox.GetMessageAsync(uid, token);
                 return new EmailMessage { Uid = uid, Message = msg };
             }).Select(task => task.Result).ToList();

            return messages;
        }

        private async Task MarkMessageSeenAsync(UniqueId uid, CancellationToken token)
        {
            await mImapClient.Inbox.AddFlagsAsync(uid, MessageFlags.Seen, true, token);
        }

        public async Task DeleteMessageAsync(UniqueId uid, CancellationToken token)
        {
            await mImapClient.Inbox.AddFlagsAsync(uid, MessageFlags.Deleted, true, token);
            await mImapClient.Inbox.ExpungeAsync(token);
        }

        private async Task DisconnectAsync(CancellationToken token)
        {
            await mImapClient.DisconnectAsync(true, token);
        }

        private async Task ConnectAsync(EmailSettings settings, CancellationToken token)
        {
            if (mImapClient.IsConnected)
                return;

            await mImapClient.ConnectAsync(settings.ServerHost, settings.Port,
                settings.UseImapSecureOptions ? 
                    MailKit.Security.SecureSocketOptions.Auto
                    : MailKit.Security.SecureSocketOptions.None, token);

            await mImapClient.AuthenticateAsync(settings.Username, settings.Password, token);
        }
    }
}