using System;
using System.Collections.Generic;

namespace ReportService.Interfaces
{
    public interface IDefaultTaskExporter
    {
        string GetDefaultView(string taskName, string dataSet);
        void SendError(List<Tuple<Exception, string>> exceptions, string taskName);
        void ForceSend(string defaultView, string taskName, string mailAddress);
    }
}
