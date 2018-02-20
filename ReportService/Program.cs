using Autofac;
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

            IPostMaster post = cont.Resolve<IPostMaster>();
            post.Send("<><>><>>fgsdfgsdgsd");

            //IDataExecutor dataex = cont.Resolve<IDataExecutor>();

            //var js = dataex.Execute("select * from instance");

            //IViewExecutor view = cont.Resolve<IViewExecutor>();

            //view.Execute(1,js);

            Console.ReadLine();
        }
    }
}

