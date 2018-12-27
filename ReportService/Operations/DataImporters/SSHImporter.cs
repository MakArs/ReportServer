using System.IO;
using System.Threading.Tasks;
using OfficeOpenXml;
using Renci.SshNet;
using ReportService.Interfaces.Core;
using ReportService.Interfaces.ReportTask;
using ReportService.Protobuf;

namespace ReportService.Operations.DataImporters
{
    public class SshImporter : IOperation
    {
        public CommonOperationProperties Properties { get; set; } = new CommonOperationProperties();
        public string Host = @"10.0.10.205";
        public string Login = "tester";
        public string Password = "password";

        public void Execute(IRTaskRunContext taskContext)
        {
            using (var client = new SftpClient(Host, Login, Password))
            {
                client.Connect();
                using (var mstr = new MemoryStream())
                {
                    client.DownloadFile(@"412412\testexcelimporter.xlsx", mstr);
                    using (var pack = new ExcelPackage(mstr))
                    {

                        var packageBuilder = new ProtoPackageBuilder();

                        var excelPars = new ExcelPackageReadingParameters
                        {
                            SkipEmptyRows = true,
                            ColumnList = new[] {"A", "B"},
                            UseColumnNames = true,
                            FirstDataRow = 1,
                            MaxRowCount = 1500
                        };

                        var package = packageBuilder.GetPackage(pack, excelPars);
                    }
                }

                var tt = client.ReadAllText(@"412412\newfile2.txt");
            }
        }

        public Task ExecuteAsync(IRTaskRunContext taskContext)
        {
            using (var client = new SftpClient(Host, Login, Password))
            {
                client.Connect();
                client.ReadAllText("dsa");
            }

            return null;
        }
    }
}