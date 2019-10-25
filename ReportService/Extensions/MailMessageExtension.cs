using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using ReportService.Entities;

namespace ReportService.Extensions
{
    public static class MailMessageExtension
    {
        public static void AddRecipientsFromGroup(this MailMessage msg,
            RecipientAddresses addresses)
        {
            AddAddressesToCollection(addresses.To, msg.To);

            AddAddressesToCollection(addresses.Bcc, msg.Bcc);
        }

        private static void AddAddressesToCollection(List<string> addresses,
            MailAddressCollection col)
        {
            if (addresses == null) return;

            foreach (var address in addresses)
                if (!string.IsNullOrEmpty(address))
                    col.Add(new MailAddress(address));
        }

        public static void AddRecipientsFromPackage(this MailMessage msg, OperationPackage package)
        {
            List<string> to = new List<string>();
            List<string> bcc = new List<string>();

            var set = package.DataSets.FirstOrDefault();
            if (set == null)
                return;

            var addressIndex = set.Columns.IndexOf(set.Columns
                .FirstOrDefault(col => col.Name == "Address"));
            var recTypeIndex = set.Columns.IndexOf(set.Columns
                .FirstOrDefault(col => col.Name == "RecType"));

            if (addressIndex == -1 || recTypeIndex == -1)
                return;

            foreach (var row in set.Rows)
            {
                var newAddrs = row.Values[addressIndex].StringValue
                    .Split(new[] {';'}, StringSplitOptions.RemoveEmptyEntries)
                    .ToList();

                if (newAddrs.Count == 0) continue;

                var recType = row.Values[recTypeIndex].StringValue;

                if (recType == "To")
                    to.AddRange(newAddrs);
                else if (recType == "Bcc")
                    bcc.AddRange(newAddrs);
            }

            AddAddressesToCollection(to, msg.To);
            AddAddressesToCollection(bcc, msg.Bcc);
        }
    }
}