using System;
using Nancy;
using Nancy.Hosting.Self;
using ReportService.Interfaces;

namespace ReportService.Implementations
{
    public class HostHolder : IHostHolder
    {
        private NancyHost nanHost = new NancyHost(new Uri($"http://localhost:12345/"));

        public HostHolder() { }

        public void Start()
        {
            nanHost.Start();
        }

        public void Stop()
        {
            nanHost.Stop();
        }
    }

    public class ReportStatusModule : NancyModule
    {
        public ILogic logic_;
        public ReportStatusModule(IViewExecutor someView, IDataExecutor someData, IConfig conf, ILogic logic)
        {
            logic_ = logic;
            string htmlWrap = @"<!DOCTYPE html>
 <html>
 <head>
 
     <title> Reports..</title>
 
     <link rel = ""stylesheet"" href = ""https://maxcdn.bootstrapcdn.com/bootstrap/3.3.7/css/bootstrap.min.css"">
    
        <style>
            table {
                border - collapse: collapse;
                width: 100 %;
            }

            th, td {
                border: 1px solid Black;
                padding: 10px;
            }
    </style>
</head>
<body>
    <h3 align = ""center""> Testing Razor </h3>
     
         <table class=""table table-bordered table-hover"">
        <tr>
            @foreach(var header in @Model.Headers)
        {
            <th> @header </th>
            }
        </tr>
        @foreach(var props in @Model.Content)
        {
        <tr>
            @foreach(var prop in @props)
            {
             <td> @prop </td>
            }
        </tr>
        }
    </table>
</body>
</html>";
            Get["/report"] = parameters =>
            {
                return $"{someView.Execute(htmlWrap, someData.Execute("select * from task", 5))}";
            };

            Get["/send"] = parameters =>
            {
                int id = Request.Query.id;
                string mail = Request.Query.address;

                string sentReps = logic.ForceExecute(id, mail);
                return sentReps != "" ? $"Reports {sentReps} sent!" : "No reports for those ids found...";
            };

            Get["/report/{id:int}"] = parameters =>
            {
                return $"{someView.Execute(htmlWrap, someData.Execute($"select * from instance_new where taskid={parameters.id}", 5))}";
            };
        }
    }
}
