using System;

namespace ReportService.Api.Models
{
    public class TimePeriod
    {
        public DateTime DateFrom { get; set; }
        public DateTime DateTo { get; set; }

        public int timeDifference;

        public void UpdateTimeDifferenc()
        {
            this.timeDifference = DateTo.Subtract(DateFrom).Days;
        }
    }
}
