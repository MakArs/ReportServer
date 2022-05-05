using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using Ardalis.GuardClauses;
using ReportService.Entities;

namespace ReportService.Extensions
{
    public static class MailMessageExtensions
    {
        public static void AddRecipientsFromRecipientAddresses(this MailMessage msg, RecipientAddresses addresses)
        {
            Guard.Against.Null(addresses, nameof(addresses));

            AddAddressesToCollection(addresses.To, msg.To);
            AddAddressesToCollection(addresses.Bcc, msg.Bcc);
        }

        private static void AddAddressesToCollection(List<string> addresses, MailAddressCollection col)
        {
            if (addresses == null) return;

            foreach (var address in addresses)
                if (!string.IsNullOrEmpty(address))
                    col.Add(new MailAddress(address));
        }

        public static void AddRecipientsFromOperationPackage(this MailMessage msg, OperationPackage package)
        {
            Guard.Against.Null(package, nameof(package));

            DataSet dataset = package.DataSets.FirstOrDefault();
            if (dataset == null) 
                return;

            List<string> toAddresses = new List<string>();
            List<string> bccAddresses = new List<string>();

            ParseDataSet(dataset, toAddresses, bccAddresses);

            AddAddressesToCollection(toAddresses, msg.To);
            AddAddressesToCollection(bccAddresses, msg.Bcc);
        }

        private static void ParseDataSet(DataSet dataset, List<string> toAddresses, List<string> bccAddresses)
        {
            var addressIndex = dataset.Columns.IndexOf(dataset.Columns.FirstOrDefault(col => col.Name == "Address"));
            var recTypeIndex = dataset.Columns.IndexOf(dataset.Columns.FirstOrDefault(col => col.Name == "RecType"));

            if (addressIndex == -1 || recTypeIndex == -1)
                return;

            foreach (var row in dataset.Rows)
            {
                var newAddrs = row.Values[addressIndex].StringValue
                    .Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries)
                    .ToList();

                if (newAddrs.Count == 0)
                    continue;

                var recType = row.Values[recTypeIndex].StringValue;

                if (recType == "To")
                    toAddresses.AddRange(newAddrs);
                else if (recType == "Bcc")
                    bccAddresses.AddRange(newAddrs);
            }
        }
    }
}
