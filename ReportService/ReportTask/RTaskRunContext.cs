using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading;
using Google.Protobuf;
using ReportService.Entities;
using ReportService.Interfaces.Core;
using ReportService.Interfaces.Operations;
using ReportService.Interfaces.ReportTask;

namespace ReportService.ReportTask
{
    public class RTaskRunContext : IRTaskRunContext
    {
        public Dictionary<string, OperationPackage> Packages { get; set; } =
            new Dictionary<string, OperationPackage>();

        public List<string> PackageStates { get; set; }

        public List<IOperation> OpersToExecute { get; set; }

        public int TaskId { get; set; }
        public DtoTaskInstance TaskInstance { get; set; }
        public CancellationTokenSource CancelSource { get; set; }
        public string TaskName { get; set; }
        public IDefaultTaskExporter Exporter { get; set; }
        public Dictionary<string, object> Parameters { get; set; }

        public string DataFolderPath => Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) +
                                        $@"\ReportServer\{TaskInstance.Id}";

        private Regex paramName;
        private readonly IArchiver archiver;

        public RTaskRunContext(IArchiver archiver)
        {
            this.archiver = archiver;
            paramName = new Regex(@"\B\@RepPar\w*\b");
        }

        public void CreateDataFolder()
        {
            Directory.CreateDirectory(DataFolderPath);
        }

        public void RemoveDataFolder()
        {
            DirectoryInfo di = new DirectoryInfo(DataFolderPath);

            foreach (FileInfo file in di.GetFiles())
            {
                file.Delete();
            }

            foreach (DirectoryInfo dir in di.GetDirectories())
            {
                dir.Delete(true);
            }

            di.Delete();
        }

        public byte[] GetCompressedPackage(string packageName)
        {
            using (var stream = new MemoryStream())
            {
                Packages[packageName].WriteTo(stream);
                return archiver.CompressStream(stream);
            }
        }

        public string SetQueryParameters(List<object> parametersList, string innerString)
        {
            var selections = paramName.Matches(innerString);

            int i = 0;

           var outerString = paramName.Replace(innerString, repl =>
            {
                var sel = selections[i].Value;

                if (!Parameters.ContainsKey(sel))
                    throw new DataException($"There is no parameter {sel} in the task");

                var paramValue = Parameters[sel];
                parametersList.Add(paramValue);

                i++;

                return $"@p{i - 1}";
            });

            return outerString;
        }

        public string SetStringParameters(string innerString)
        {
            var outerString = paramName.Replace(innerString, repl =>
            {
                if (!Parameters.ContainsKey(repl.Value))
                    throw new DataException($"There is no parameter {repl.Value} in the task");


                if (Parameters[repl.Value] is DateTime dateTimeValue &&
                    dateTimeValue.TimeOfDay == TimeSpan.Zero)
                    return $"{dateTimeValue:dd.MM.yy}";
                else
                return Parameters[repl.Value].ToString();
            });

            return outerString;
        }
    }
}