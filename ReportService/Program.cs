using Autofac;
using ReportService.Implementations;
using ReportService.Interfaces;
using System;

namespace ReportService
{
    class Program
    {
        static void Main(string[] args)
        {
            BootsTrap.Init();
            var cont = BootsTrap.Container;

            ILogic log = cont.Resolve<ILogic>();

            log.Execute();
            //IDataExecutor dataex = cont.Resolve<IDataExecutor>();

            //var js = dataex.Execute("select * from instance");

            //IViewExecutor view = cont.Resolve<IViewExecutor>();

            //IPostMaster post = cont.Resolve<IPostMaster>();
            //post.Send(view.Execute(1, js), "anikeev@smartdriving.io");

            //IHostHolder host = cont.Resolve<IHostHolder>();

           // host.Start();

            Console.ReadLine();
        }
    }
}

