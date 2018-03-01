using ReportService.Implementations;
using System;

namespace ReportService
{
    class Program
    {
        static void Main(string[] args)
        {
            //BootsTrap.Init();
            //var cont = BootsTrap.Container;
            //ILogic log = cont.Resolve<ILogic>();
            //log.Execute();

            HostHolder hld = new HostHolder();
            hld.Start();
            
            Console.ReadLine();
            hld.Stop();
        }
    }
}
