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

        public static void AddRecepientsFromDataSet(this MailMessage msg, string dataSet)
        {
            List<string> To = new List<string>();
            List<string> Bcc = new List<string>();

            JArray jObj = JArray.Parse(dataSet);
            foreach (JObject row in jObj.Children<JObject>())
            {
                var newAddrs = row.Properties()
                    .FirstOrDefault(pr => pr.Name == "Address")?
                    .Value.ToString().Split(new[] {';'},
                        StringSplitOptions.RemoveEmptyEntries).ToList();

                if (newAddrs != null && row.Properties().FirstOrDefault(pr => pr.Name == "RecType")?
                        .Value.ToString() == "To")
                    To.AddRange(newAddrs);

                else if (newAddrs != null && row.Properties()
                             .FirstOrDefault(pr => pr.Name == "RecType")?
                             .Value.ToString() == "Bcc")
                    Bcc.AddRange(newAddrs);
            }

            AddAddressesToCollection(To, msg.To);
            AddAddressesToCollection(Bcc, msg.Bcc);
        }
    }
}