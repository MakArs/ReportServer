using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using Gerakul.ProtoBufSerializer;

namespace ReportService.Interfaces
{
    public interface IDescriptorBuilder
    {
        DataSetDescriptor GetClassDescriptor<T>() where T : class;
        DataSetDescriptor GetDbReaderDescriptor(DbDataReader reader);
    }

    public class DataSetDescriptor
    {
        public readonly Dictionary<int, ColumnInfo> Fields = new Dictionary<int, ColumnInfo>();

        public static MessageDescriptor<DataSetDescriptor> GetDescriptor()
        {
            return MessageDescriptorBuilder.New<DataSetDescriptor>()
                .MessageArray(2, di => di.Fields.Select(field =>
                        new ColumnInfoPair(field.Key, field.Value)),
                    (di, newfield) => di.Fields.Add(newfield.Tag, newfield.ColumnInfo),
                    ColumnInfoPair.GetDescriptor())
                .CreateDescriptor();
        }
    }

    public class ColumnInfoPair
    {
        public int Tag;
        public ColumnInfo ColumnInfo;

        public ColumnInfoPair()
        {
        }

        public ColumnInfoPair(int tag, ColumnInfo info)
        {
            Tag = tag;
            ColumnInfo = info;
        }

        public static MessageDescriptor<ColumnInfoPair> GetDescriptor()
        {
            return MessageDescriptorBuilder.New<ColumnInfoPair>()
                .Int32(1, cip => cip.Tag, (cip, tag) => cip.Tag = tag)
                .Message(2, cip => cip.ColumnInfo, (cip, info) => cip.ColumnInfo = info,
                    ColumnInfo.GetDescriptor())
                .CreateDescriptor();
        }
    }

    public class ColumnInfo
    {
        public string Name;
        public string TypeName;

        public ColumnInfo()
        {
        }

        public ColumnInfo(string name, Type columnType)
        {
            Name = name;
            TypeName = columnType.FullName;
        }


        public static MessageDescriptor<ColumnInfo> GetDescriptor()
        {
            return MessageDescriptorBuilder.New<ColumnInfo>()
                .String(1, ci => ci.Name, (ci, name) => ci.Name = name)
                .String(2, ci => ci.TypeName, (ci, type) => ci.TypeName = type)
                .CreateDescriptor();
        }
    }
}