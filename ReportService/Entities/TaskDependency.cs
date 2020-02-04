namespace ReportService.Entities
{
    public class TaskDependency
    {
        public long TaskId { get; set; }
        public int MaxSecondsPassed { get; set; }
    }
}