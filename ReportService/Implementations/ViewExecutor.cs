using ReportService.Interfaces;

namespace ReportService.Models
{
    public class ViewExecutor : IViewExecutor
    {
        //TODO: add view templates 
        public ViewExecutor() { }

        public string Execute(int viewTemplate, string json)
        {
            return json.ToString();//+convert json to html table
        }
    }
}
