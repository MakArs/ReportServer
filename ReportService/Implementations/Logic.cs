using System;
using ReportService.Interfaces;

namespace ReportService.Models
{
    public class Logic : ILogic
    {
        private IConfig config_;
        private IDataExecutor DataEx;
        private IViewExecutor ViewEx;
        private IPostMaster PostMaster;
        // TODO: addschedule templates

        public Logic(IConfig config, IDataExecutor dataEx, IViewExecutor viewEx, IPostMaster postMaster)
        {
            config_ = config;
            DataEx = dataEx;
            ViewEx = viewEx;
            PostMaster = postMaster;
        }

        public void Execute()
        {
            for (int i = 0; ; i++)
            {
                if (i % 60 == 0) config_.Reload();

                foreach (ReportTask task in config_.GetTasks())
                {
                    var repInstance = DataEx.Execute(task.SendAddress);
                    var viewInstance = ViewEx.Execute(task.ViewTemplateID, repInstance);
                    if (task.ScheduleID > 0) PostMaster.Send(viewInstance);//+TODO: realize schedule templates
                }
            }
        }

        public void Stop()
        {
            throw new NotImplementedException(); //TODO: some stop logic(?)
        }
    }
}
