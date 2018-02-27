using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Autofac;
using ReportService.Interfaces;

namespace ReportService.Implementations
{
    /* 
     * Functions:
     * 1. Task list prepare and update
     * 2. Control schedules
     * 3. 
     */
    public class Logic : ILogic
    {
        private IConfig config_;
        private List<RTask> tasks_;
        private IHostHolder holder_;
        private IContainer autofac_;

        public Logic(IContainer aAutofac, IConfig config)
        {
            autofac_ = aAutofac;
            config_ = config;
            tasks_ = new List<RTask>();
        }

        private void UpdateTaskList()
        {
            tasks_.Clear();

            foreach (var dto_task in config_.GetTasks())
            {
                var task = autofac_.Resolve<IRTask>(
                    new NamedParameter("ID", dto_task.ID),
                    new NamedParameter("aTemplateID", dto_task.ViewTemplateID),
                    new NamedParameter("aScheduleID", dto_task.ScheduleID),
                    new NamedParameter("aQuery", dto_task.Query),
                    new NamedParameter("aSendAddress", dto_task.SendAddress));
                tasks_.Add((RTask)task);
            }
        }

        public void Execute()
        {
            UpdateTaskList();
            holder_ = autofac_.Resolve<IHostHolder>();
            holder_.Start();

            Stopwatch stepTimer = new Stopwatch();
            Stopwatch sumTimer = new Stopwatch();
            double reloadTrigger = 0;
            stepTimer.Start();
            sumTimer.Start();

            for (int i = 1; i < 1000; i++)
            {
                Task.Delay(TimeSpan.FromSeconds(1)).Wait();
                Console.WriteLine($"Step {i}. Passed from previous step: {stepTimer.Elapsed} seconds. Total time passed: {sumTimer.Elapsed}");
                stepTimer.Restart();

                if (sumTimer.ElapsedMilliseconds / 1000 - reloadTrigger > 60)
                {
                    config_.Reload();
                    UpdateTaskList();
                    reloadTrigger = sumTimer.ElapsedMilliseconds / 1000;
                }

                // TODO: schedule support
                foreach (RTask task in tasks_)

                {
                    if (DateTime.Now.ToString("hh:mm:ss")=="13:38:00")
                        task.Execute();
                }
            }

            holder_.Stop();
        }

        public string ForceExecute(int[] aTaskIDs)
        {
            string executed = "";
            foreach (RTask task in tasks_.Where(t => aTaskIDs.Contains(t.ID)))
            {
                task.Execute();
                executed += $",{task.ID}";
            }
            return executed;
        }

        public void Stop()
        {
            // TODO: some stop logic(?)
            throw new NotImplementedException();
        }

    }
}
