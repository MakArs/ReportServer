using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using Newtonsoft.Json.Linq;

namespace ReportService.Extensions
{
    public class RecepientAddresses
    {
        public List<string> To = new List<string>();
        public List<string> Bcc = new List<string>();
    }

    public static class MailMessageExtension
    {
        public static void AddRecepientsFromGroup(this MailMessage msg,
                                                  RecepientAddresses addresses)
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

        public static void AddRecepientsFromPackage(this MailMessage msg, OperationPackage package)
        {
            List<string> To = new List<string>();
            List<string> Bcc = new List<string>();

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
                    To.AddRange(newAddrs);
                else if (recType == "Bcc")
                    Bcc.AddRange(newAddrs);
            }

            AddAddressesToCollection(To, msg.To);
            AddAddressesToCollection(Bcc, msg.Bcc);
        }
    }
}