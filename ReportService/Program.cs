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
            //ILogic logic = BootsTrap.Container.Resolve<ILogic>();

            IConfig conf = BootsTrap.Container.Resolve<IConfig>();
            foreach (var l in conf.GetTasks()) Console.WriteLine($"Task number {l.ID}, schedule {l.ScheduleID},view template {l.ViewTemplateID},email {l.SendAddress}");
            Console.WriteLine("reloading tasks...");
            conf.Reload();
            foreach (var l in conf.GetTasks())
            {
                Console.WriteLine($"Task number {l.ID}, schedule {l.ScheduleID},view template {l.ViewTemplateID},email {l.SendAddress}");
                Console.WriteLine(conf.SaveInstance(l.ID, $"json{l.ID}", $"html{l.ID}"));
            }

            Console.ReadLine();
        }
    }
}
