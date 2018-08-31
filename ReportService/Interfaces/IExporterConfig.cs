using System.Collections.Generic;

namespace ReportService.Interfaces
{
    public interface IExporterConfig
    {
        int Id { get; set; }
        string Name { get; set; }
        List<string> DataTypes { get; set; }
    }
}
