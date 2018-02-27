using ReportService.Interfaces;

namespace ReportService.Implementations
{
    public class RTask : IRTask
    {
        public int ID { get; set; }
        public string SendAddress { get; set; }
        public int ViewTemplateID { get; set; }
        public int ScheduleID { get; set; }
        public string Query { get; set; }
        private IDataExecutor dataEx_;
        private IViewExecutor viewEx_;
        private IPostMaster postMaster_;

        public RTask(IDataExecutor aDataEx, IViewExecutor aViewEx, IPostMaster aPostMaster,
            int ID, int aTemplateID, int aScheduleID, string aQuery, string aSendAddress)
        {
            dataEx_ = aDataEx;
            viewEx_ = aViewEx;
            postMaster_ = aPostMaster;
            this.ID = ID;
            Query = aQuery;
            ViewTemplateID = aTemplateID;
            SendAddress = aSendAddress;
            ScheduleID = aScheduleID;
        }

        public void Execute()
        {
            string jsonReport = dataEx_.Execute(Query);
            string htmlReport = viewEx_.Execute(ViewTemplateID, jsonReport);
            postMaster_.Send(htmlReport, SendAddress);
        }
    }
}
