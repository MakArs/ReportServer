using System.Data.Common;
using System.Linq;
using ReportService.Interfaces.Protobuf;

namespace ReportService.Core
{
    public class DescriptorBuilder : IDescriptorBuilder
    {
        public DataSetDescriptor GetClassDescriptor<T>() where T : class
        {
            var innerFields = typeof(T).GetFields();

            var descriptor = new DataSetDescriptor();

            for (int i = 0; i < innerFields.Length; i++)
            {
                var field = innerFields[i];
                descriptor.Fields.Add(i + 1,
                    new ColumnInfo(field.Name, field.FieldType));
            }

            return descriptor;
        }

        public DataSetDescriptor GetDbReaderDescriptor(DbDataReader reader)
        {
            var descriptor = new DataSetDescriptor();

            var colCount = reader.FieldCount;

            var names = reader.GetColumnSchema().Select(sch => sch.ColumnName).ToArray();

            var typeNames = reader.GetColumnSchema().Select(sch => sch.DataType).ToArray();

            for (int i = 0; i < colCount; i++)
            {
                descriptor.Fields.Add(i + 1, new ColumnInfo(names[i], typeNames[i]));
            }

            return descriptor;
        }
    }
}
