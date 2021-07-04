using MimeKit;
using ReportService.Entities;
using System.Threading;
using System.Threading.Tasks;

namespace ReportService.Interfaces.Core
{
    public interface IEmailClientService
    {
        Task<MimePart> GetLastMatchingFileFromEmail(EmailSettings settings, CancellationToken token);
    }
}