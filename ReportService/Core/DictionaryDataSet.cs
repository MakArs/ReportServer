using System.Collections.Generic;
using ReportService.Interfaces;

namespace ReportService.Core
{
    public class DataSet
    {
        private readonly DataSetDescriptor descriptor;

        public List<object[]> Rows;

        public List<DataSetRow> GetAllRows()
        {
            return null;
        }

        public DataSetRow GetRow(int index)
        {
            return null;
        }
    }

    public partial class ElementarySerializer
    {
        public DataSetDescriptor SaveHeaderFromClassFields<T>() where T : class
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
    }
}