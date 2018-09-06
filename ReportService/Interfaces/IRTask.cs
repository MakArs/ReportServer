using ReportService.Extensions;
using System;
using System.Collections.Generic;

namespace ReportService.Interfaces
{
    public enum RReportType : byte
    {
        Common = 1,
        Custom = 2
    }

    public enum InstanceState
    {
        InProcess = 1,
        Success = 2,
        Failed = 3
    }

    public class RRecepientGroup
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Addresses { get; set; }
        public string AddressesBcc { get; set; }

        public RecepientAddresses GetAddresses()
        {
            return new RecepientAddresses
            {
                To = Addresses.Split(';'),
                Bcc = AddressesBcc?.Split(';')
            };
        }
    }

    public class RFullInstance
    {
        public int Id { get; set; }
        public string Data { get; set; }
        public string ViewData { get; set; }
        public int TaskId { get; set; }
        public DateTime StartTime { get; set; }
        public int Duration { get; set; }
        public int State { get; set; }
        public int TryNumber { get; set; }
    }

    public class RInstanceData
    {
        public int InstanceId { get; set; }
        public string Data { get; set; }
        public string ViewData { get; set; }
    }

    public interface IRTask
    {
        int Id { get; }
        string Name { get; }
        DtoSchedule Schedule { get; }
        DateTime LastTime { get; }
        Dictionary<string, string> DataSets { get; }
        List<IOperation> Operations { get; set; }

        void Execute();
        void UpdateLastTime();
        string GetCurrentView();
    }
}