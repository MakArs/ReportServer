using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ReportService.Api.Models
{
    public class RequestStatusFilter
    {
        public long[] TaskIds { get; set; }
        public long[] TaskRequestInfoIds { get; set; }
        public int? Status { get; set; }
        public TimePeriod TimePeriod { get; set; }
    }
}
