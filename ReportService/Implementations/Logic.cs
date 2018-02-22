using System;
using ReportService.Interfaces;

namespace ReportService.Implementations
{
    public class Logic : ILogic
    {
        private IConfig config_;
        private IDataExecutor dataExecutor_;
        private IViewExecutor viewExecutor_;
        private IPostMaster postMaster_;

        public Logic(IConfig config, IDataExecutor dataEx, IViewExecutor viewEx, IPostMaster postMaster)
        {
            config_ = config;
            dataExecutor_ = dataEx;
            viewExecutor_ = viewEx;
            postMaster_ = postMaster;
        }

        public void Execute()
        {
            for (int i = 0; ; i++)
            {
                if (i % 60 == 0) config_.Reload();

                foreach (ReportTask task in config_.GetTasks())
                {
                    var jsonString = dataExecutor_.Execute(task.SendAddress);
                    var htmlString = viewExecutor_.Execute(task.ViewTemplateID, jsonString);

                    // TODO: realize schedule templates
                    if (task.ScheduleID > 0)
                        postMaster_.Send(htmlString,"");
                }
            }
        }

        public void Stop()
        {
            // TODO: some stop logic(?)
            throw new NotImplementedException();
        }
    }
}
