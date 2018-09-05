using System;

namespace ReportService.Interfaces
{
    public interface ICleanupSettings
    {
       DateTime KeepingTime { get; set; } 
    }
}
