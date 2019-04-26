using System;
using Nancy.Swagger.Annotations.Attributes;

namespace ReportService.Nancy.Models
{
    [Model("Operation instance")]
    public class ApiOperInstance
    {
        public int Id { get; set; }
        public int TaskInstanceId { get; set; }
        public int OperationId { get; set; }
        public DateTime StartTime { get; set; }
        public int Duration { get; set; }
        public int State { get; set; }
        public byte[] DataSet { get; set; }
        public string ErrorMessage { get; set; }
        public string OperName { get; set; }
    }
}