using System.Threading.Tasks;
using ReportService.Interfaces.ReportTask;

namespace ReportService.Interfaces.Core
{
    public interface IOperation
    {
        CommonOperationProperties Properties { get; set; }
        void Execute(IRTaskRunContext taskContext);
        Task ExecuteAsync(IRTaskRunContext taskContext);
    }

    public interface IOperationConfig
    {
        string PackageName { get; set; }
    }

    public class CommonOperationProperties
    {
        public int Id { get; set; }
        public bool IsDefault { get; set; }
        public int Number { get; set; }
        public string Name { get; set; }
        public string PackageName { get; set; }
    }
}