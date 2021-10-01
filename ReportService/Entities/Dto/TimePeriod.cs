using System;
using System.Collections.Generic;
using System.Text;

namespace ReportService.Entities.Dto
{
    public class TimePeriod
    {
        public DateTime DateFrom { get; set; }
        public DateTime DateTo { get; set; }

        public int timeDifference;
    }
}
