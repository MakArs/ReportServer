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
            Console.ReadLine();
        }
    }
}

