using System;
using ReportService.Entities;

namespace ReportService.Api.Models
{
    public class TaskRequestInfo
    {
        public long? RequestId { get; set; }
        public long TaskId { get; set; }
        public long? TaskInstanceId { get; set; }
        public TaskParameter[] Parameters { get; set; }
        public DateTime CreateTime { get; set; }
        public DateTime UpdateTime { get; set; }
        public RequestStatus Status { get; set; }
        
        public TaskRequestInfo(long taskId, TaskParameter[] taskParameters, RequestStatus status = RequestStatus.Pending)
        {
            this.TaskId = taskId;
            this.Parameters = taskParameters;
            this.CreateTime = DateTime.UtcNow;
            this.UpdateTime = DateTime.UtcNow;
            this.Status = status;
        }
    }
}