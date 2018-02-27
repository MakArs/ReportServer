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

        public Logic(IContainer aAutofac, IConfig config, IHostHolder holder)
        {
            autofac_ = aAutofac;
            config_ = config;
            holder_ = holder;
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

            holder_.Start();  // TODO: create instance by init Logic?

            Stopwatch sw = new Stopwatch();
            sw.Start();

            for (int i = 1; i < 59; i++)
            {
                Task.Delay(TimeSpan.FromSeconds(1)).Wait();
                Console.WriteLine($"Step {i}. Passed from previous step: {sw.Elapsed} seconds");
                sw.Restart();

                if (i % 60 == 0)
                {
                    config_.Reload();
                    UpdateTaskList();
                }

                // TODO: schedule support
                foreach (RTask task in tasks_)
                    task.Execute();
            }

            holder_.Stop();
        }

        public void ForceExecute(int[] aTaskIDs)
        {
            foreach (RTask task in tasks_.Where(t => aTaskIDs.Contains(t.ID)))
            {
                if (task.ScheduleID > 0)
                    task.Execute();
            }
        }

        public void Stop()
        {
            // TODO: some stop logic(?)
            throw new NotImplementedException();
        }

    }
}
