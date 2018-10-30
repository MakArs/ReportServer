using System;
using System.Collections.Generic;
using Gerakul.ProtoBufSerializer;
using ReportService.Interfaces.Protobuf;

namespace ReportService.Core
{
    public class DataSet
    {
        public  DataSetDescriptor Descriptor;

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

    public class DataSetRow
    {
        public string Key;
        public object Value;

        public DataSetRow()
        {
        }

        public DataSetRow(string key, object value)
        {
            Key = key;
            Value = value;
        }

        public static MessageDescriptor<DataSetRow> GetDescriptor(string typeName)
        {
            var builder = MessageDescriptorBuilder.New<DataSetRow>()
                .String(1, dsr => dsr.Key, (dsr, newName) => dsr.Key = newName);

            switch (typeName)
            {
                case "Boolean":
                    builder.Bool(2, dsr => Convert.ToBoolean(dsr.Value),
                        (dsr, newVal) => dsr.Value = newVal);
                    break;

                case "Double":
                    builder.Double(2, dsr => Convert.ToDouble(dsr.Value),
                        (dsr, newVal) => dsr.Value = newVal);
                    break;

                case "String":
                    builder.String(2, dsr => dsr.Value.ToString(),
                        (dsr, newVal) => dsr.Value = newVal);
                    break;

                case "Int32":
                    builder.Int32(2, dsr => Convert.ToInt32(dsr.Value),
                        (dsr, newVal) => dsr.Value = newVal);
                    break;
            }

            return builder.CreateDescriptor();
        }
    }
}