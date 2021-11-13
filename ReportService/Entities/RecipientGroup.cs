using System;
using System.Linq;

namespace ReportService.Entities
{
    public class RecipientGroup
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Addresses { get; set; }
        public string AddressesBcc { get; set; }

        public RecipientAddresses GetAddresses()
        {
            return new RecipientAddresses
            {
                To = Addresses?.Split(new[] {';'}, StringSplitOptions.RemoveEmptyEntries).Where(IsValidEmail).ToList(),

                Bcc = AddressesBcc?.Split(new[] {';'}, StringSplitOptions.RemoveEmptyEntries).Where(IsValidEmail).ToList()
            };
        }

        private bool IsValidEmail(string email)
        {
            bool isValid;

            try
            {
                var address = new System.Net.Mail.MailAddress(email);
                isValid = address.Address == email;
            }
            catch
            {
                Console.WriteLine($"{email} is not a valid email address and it won't be added to the recipient list");
                isValid = false;
            }

            return isValid;
        }
    }
}
