using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Autofac;
using Google.Protobuf;
using Nancy.Hosting.Self;
using NUnit.Framework;
using ReportService;
using ReportService.Core;
using ReportService.Entities;
using ReportService.Entities.Dto;
using ReportService.Interfaces.Operations;
using ReportService.Interfaces.ReportTask;
using ReportService.Operations.DataExporters.Configurations;
using ReportService.Operations.DataImporters.Configurations;

namespace TestModule
{
    [TestFixture]
    public class Operations
    {
        private readonly ILifetimeScope autofac;

        public Operations()
        {
            HostConfiguration hostConfigs = new HostConfiguration()
            {
                UrlReservations = new UrlReservations {CreateAutomatically = true},
                RewriteLocalhost = true
            };

            var nancyHost = new NancyHost(
                new Uri("http://localhost:12345"),
                new Bootstrapper(),
                hostConfigs);

            autofac = Bootstrapper.Global.Resolve<ILifetimeScope>();
        }


        public void HistoryImporterTest()
        {
            var histImp = autofac.ResolveNamed<IOperation>("CommonHistoryImporter",
                new NamedParameter("config", new HistoryImporterConfig
                {
                    PackageName = "histPack",
                    OperInstanceId = 21440
                }));

            var taskContext = autofac.Resolve<IReportTaskRunContext>();

            var dtoTaskInstance = new DtoTaskInstance
            {
                Id = 151256,
                StartTime = DateTime.Now,
                Duration = 0,
                State = (int) InstanceState.InProcess
            };
            taskContext.TaskInstance = dtoTaskInstance;

            histImp.Execute(taskContext);
        }

        private Dictionary<MergedRow, object> GetMergedRows(List<List<object>> rows, List<int> groupCols,
            List<int> currGroupCols)
        {
            Dictionary<MergedRow, object> dict;

            var newGroupCols = currGroupCols.Skip(1).ToList();

            if (newGroupCols.Any())
            {
                dict = rows.GroupBy(row => row[currGroupCols[0]])
                    .ToDictionary(group => new MergedRow
                    {
                        Value = group.Key,
                        SpanCount = group.Count()
                    }, group => (object) group.ToList());

                for (int i = 0; i < dict.Count; i++)
                {
                    var key = dict.Keys.ElementAt(i);

                    if (dict[key] is List<List<object>> subGroup)
                    {
                        dict[key] = GetMergedRows(subGroup, groupCols, newGroupCols);
                    }
                }
            } //if this is not last column that needs to be grouped by, saving all data for further grouping

            else
            {
                dict = rows.GroupBy(row => row[currGroupCols[0]])
                    .ToDictionary(group => new MergedRow
                        {
                            Value = group.Key,
                            SpanCount = group.Count()
                        },
                        group => (object) group
                            .Select(list => list.Where((obj, index) => !groupCols.Contains(index)).ToList()).ToList());
            } //if this is last column that needs to be grouped by, save only columns that not need grouping

            return dict;
        }


        public void TestGrouping()
        {
            var datasetc = new DataSetContent
            {
                GroupColumns = new List<int> {3, 4, 1},
                Headers = new List<string>
                {
                    "headOf0",
                    "headOf1",
                    "headOff2",
                    "headOf3",
                    "headOf4"
                },
                Name = "testSet",
                Rows = new List<List<object>>
                {
                    new List<object> {0, 317, "squirrell", "M", 0.5},
                    new List<object> {0, 37, "squirrell", "M", 0.5},
                    new List<object> {0, 437, "Dog", "M", 0.25},
                    new List<object> {0, 337, "squirrell", "M", 0.5},
                    new List<object> {0, 37, "squirrell", "M", 0.5},
                    new List<object> {0, 37, "squirrell", "M", 0.58},
                    new List<object> {0, 37, "Cow", "M", 0.5},
                    new List<object> {0, 937, "squirrell", "M", 0.53},
                    new List<object> {0, 37, "squirrell", "M", 0.5},
                    new List<object> {0, 3127, "squirrell", "M", 0.15},
                    new List<object> {0, 237, "squirrell", "F", 0.5},
                    new List<object> {0, 37, "squirrell", "F", 0.5},
                }
            };

            var grouping = GetMergedRows(groupCols: datasetc.GroupColumns, currGroupCols: datasetc.GroupColumns,
                rows: datasetc.Rows);

            var html = CreateGroupedTable(grouping);

            Assert.That(grouping.GetType() == typeof(Dictionary<MergedRow, object>));
        }

        private string CreateGroupedTable(Dictionary<MergedRow, object> data)
        {
            string help = "";

            foreach (var group in data)
            {
                help += Environment.NewLine + $"<td rowspan={group.Key.SpanCount}>{group.Key.Value}</td>";

                if (group.Value is Dictionary<MergedRow, object> dict)
                    help += CreateGroupedTable(dict);
                //some tricky recursion. html shows rows correctly even if we do not use opening <tr> tag for the first row of rowspan

                else if (group.Value is List<List<object>> objs)
                {
                    foreach (var val in objs.First())
                        help += Environment.NewLine + $"<td>{val}</td>";

                    help += Environment.NewLine + "</tr>";
                    //ending of the first data row for all columns that not need grouping

                    foreach (var row in objs.Skip(1))
                    {
                        help += Environment.NewLine + "<tr>";

                        foreach (var val in row)
                            help += Environment.NewLine + $"<td>{val}</td>";

                        help += Environment.NewLine + "</tr>";

                    } //adding all other rows for all columns that not need grouping
                }
            }

            return help;
        }


        public void GetFileFromSshTest()
        {
            var sshImp = autofac.ResolveNamed<IOperation>("CommonSshImporter",
                new NamedParameter("config", new SshImporterConfig
                {
                    FilePath = "savedData/filename.xlsx",
                    Host = @"10.0.10.205",
                    Login = "tester",
                    Password = "password"
                }));
            var taskContext = autofac.Resolve<IReportTaskRunContext>();
            var dtoTaskInstance = new DtoTaskInstance
            {
                Id = 151256,
                StartTime = DateTime.Now,
                Duration = 0,
                State = (int) InstanceState.InProcess
            };
            taskContext.TaskInstance = dtoTaskInstance;
            sshImp.Execute(taskContext);

        }


        public IReportTaskRunContext CsvImporterTest()
        {
            var csvimp = autofac.ResolveNamed<IOperation>("CommonCsvImporter",
                new NamedParameter("config", new CsvImporterConfig
                {
                    FileFolder = @"C:\user",
                    FileName = @"filename.csv",
                    PackageName = "package0101",
                    Delimiter = ";"
                }));

            var csvimp2 = autofac.ResolveNamed<IOperation>("CommonCsvImporter",
                new NamedParameter("config", new CsvImporterConfig
                {
                    FileFolder = @"C:\user",
                    FileName = @"filename.csv",
                    PackageName = "package0102",
                    Delimiter = ","
                }));

            var csvimp3 = autofac.ResolveNamed<IOperation>("CommonCsvImporter",
                new NamedParameter("config", new CsvImporterConfig
                {
                    FileFolder = @"C:\user",
                    FileName = @"filename.csv",
                    PackageName = "package0103",
                    Delimiter = "\\t"
                }));

            var csvimp4 = autofac.ResolveNamed<IOperation>("CommonCsvImporter",
                new NamedParameter("config", new CsvImporterConfig
                {
                    FileFolder = @"C:\user",
                    FileName = @"filename.csv",
                    PackageName = "package0104",
                    Delimiter = "\\r\\n"
                }));

            var csvimp5 = autofac.ResolveNamed<IOperation>("CommonCsvImporter",
                new NamedParameter("config", new CsvImporterConfig
                {
                    FileFolder = @"C:\user",
                    FileName = @"filename.csv",
                    PackageName = "package0105",
                    Delimiter = "|"
                }));

            var csvimp6 = autofac.ResolveNamed<IOperation>("CommonCsvImporter",
                new NamedParameter("config", new CsvImporterConfig
                {
                    FileFolder = @"C:\user",
                    FileName = @"filename.csv",
                    PackageName = "package0106",
                    Delimiter = "."
                }));

            var taskContext = autofac.Resolve<IReportTaskRunContext>();

            csvimp.Execute(taskContext);
            csvimp2.Execute(taskContext);
            csvimp3.Execute(taskContext);
            csvimp4.Execute(taskContext);
            csvimp5.Execute(taskContext);
            csvimp6.Execute(taskContext);
            Assert.AreEqual(taskContext.Packages["package0101"], taskContext.Packages["package0102"]);
            Assert.AreEqual(taskContext.Packages["package0101"], taskContext.Packages["package0103"]);
            Assert.AreEqual(taskContext.Packages["package0101"], taskContext.Packages["package0104"]);
            Assert.AreEqual(taskContext.Packages["package0101"], taskContext.Packages["package0105"]);
            return taskContext;
            //Assert.AreEqual(taskContext.Packages["package0101"], taskContext.Packages["package0106"]);
        }


        public void SharpZipLibTest()
        {
            var context = CsvImporterTest();

            var archiver = new ArchiverZip();

            using (var stream = new MemoryStream())
            {
                context.Packages["package0101"].WriteTo(stream);
                var bytes = stream.ToArray();
                var compressedBytes = archiver.CompressStream(stream);
                var extractedBytes = archiver.ExtractFromByteArchive(compressedBytes);

                var b2bexp = autofac.ResolveNamed<IOperation>("CommonB2BExporter",
                    new NamedParameter("config", new B2BExporterConfig
                    {
                        PackageName = "SomeName",
                        ConnectionString =
                            "",
                        DbTimeOut = 600,
                        Description = "ZippedFile",
                        ExportInstanceTableName = "[dbo].[reportinstancetablename]",
                        ExportTableName = "[dbo].[reporttablename]",
                        ReportName = "TestRep"
                    }));
                Assert.AreEqual(extractedBytes, bytes);
                //var depressedBytes = archiver.ExtractFromByteArchive(compressedBytes);
                //Assert.AreEqual(compressedBytes,depressedBytes);
            }
        }
    }
}