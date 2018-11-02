using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using Gerakul.ProtoBufSerializer;

namespace ReportService.Interfaces.Protobuf
{
    public interface IDescriptorBuilder
    {
        DataSetParameters GetClassParameters<T>() where T : class;
        DataSetParameters GetDbReaderParameters(DbDataReader reader);
    }

    public class OperationPackage
    {
        public DateTime Created;
        public string OperationName;
        public List<Dataset> DataSets;
    }

    public class DataSetParameters
    {
        public int FieldCount;
        public readonly Dictionary<int, ColumnInfo> Fields = new Dictionary<int, ColumnInfo>();

        public static MessageDescriptor<DataSetParameters> GetDescriptor()
        {
            return MessageDescriptorBuilder.New<DataSetParameters>()
                .Int32(1, di => di.FieldCount, (di, count) => di.FieldCount = count)
                .MessageArray(2, di => di.Fields.Select(field =>
                        new ColumnInfoPair(field.Key, field.Value)),
                    (di, newfield) =>
                    {
                        if (di.Fields.Count < di.FieldCount)
                            di.Fields.Add(newfield.Tag, newfield.ColumnInfo);
                    },
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