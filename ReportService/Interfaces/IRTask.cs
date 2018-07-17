using System;

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
        public int    Id        { get; set; }
        public string Name      { get; set; }
        public string Addresses { get; set; }

        public string[] GetAddresses()
        {
            return Addresses.Split(';');
        }
    }

    public class RFullInstance
    {
        public int      Id        { get; set; }
        public string   Data      { get; set; }
        public string   ViewData  { get; set; }
        public int      TaskId    { get; set; }
        public DateTime StartTime { get; set; }
        public int      Duration  { get; set; }
        public int      State     { get; set; }
        public int      TryNumber { get; set; }
    }

    public class RInstanceData
    {
        public int    InstanceId { get; set; }
        public string Data       { get; set; }
        public string ViewData   { get; set; }
    }

    public interface IRTask
    {
        int             Id                { get; }
        string          ReportName        { get; }
        RRecepientGroup SendAddresses     { get; }
        string          ViewTemplate      { get; }
        DtoSchedule     Schedule          { get; }
        string          ConnectionString  { get; }
        long            ChatId            { get; }
        string          Query             { get; }
        int             TryCount          { get; }
        int             QueryTimeOut      { get; }
        RReportType     Type              { get; }
        int             ReportId          { get; }
        bool            HasHtmlBody       { get; }
        bool            HasJsonAttachment { get; }
        DateTime        LastTime          { get; }

        void   Execute(string address = null);
        void   UpdateLastTime();
        string GetCurrentView();
    }
}