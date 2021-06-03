using System;
using System.Collections.Generic;

namespace ReportService.Interfaces.ReportTask
{
    public interface IDefaultTaskExporter
    {
        string GetDefaultPackageView(string taskName, OperationPackage package);
        void SendError(List<Tuple<Exception, string>> exceptions, string taskName, long taskId);
        void ForceSend(string defaultView, string taskName, string mailAddress);
    }
}