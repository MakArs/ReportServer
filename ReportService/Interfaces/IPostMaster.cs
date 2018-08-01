using OfficeOpenXml;
using ReportService.Extensions;

namespace ReportService.Interfaces
{
    public interface IPostMaster
    {
        void Send(string reportName, RecepientAddresses addresses, string htmlReport = null, string jsonReport = null, ExcelPackage xlsxReport = null);
    }
}
