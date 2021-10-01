using System;
using System.Collections.Generic;
using System.Text;

namespace ReportService.Entities.Dto
{
    public class RequestStatusFilter
    {
        public long[] TaskIds { get; set; }
        public long[] TaskRequestInfoIds { get; set; }
        public RequestStatus? Status { get; set; }
        public TimePeriod TimePeriod { get; set; }
    }
}
