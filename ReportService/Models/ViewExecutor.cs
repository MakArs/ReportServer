using ReportService.Interfaces;

namespace ReportService.Models
{
    public class ViewExecutor : IViewExecutor
    {
        //+addtemplates
        public ViewExecutor() { }

        public string Execute(int viewTemplate, string json)
        {
            return json.ToString();//+do
        }
    }
}
