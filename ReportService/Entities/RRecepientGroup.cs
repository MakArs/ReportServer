using System;
using System.Linq;
using ReportService.Extensions;

namespace ReportService.Entities
{
    public class RRecepientGroup
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Addresses { get; set; }
        public string AddressesBcc { get; set; }

        public RecipientAddresses GetAddresses()
        {
            return new RecipientAddresses
            {
                To = Addresses.Split(new[] { ';' },
                    StringSplitOptions.RemoveEmptyEntries).ToList(),
                Bcc = AddressesBcc?.Split(new[] { ';' },
                    StringSplitOptions.RemoveEmptyEntries).ToList()
            };
        }
    }
}
