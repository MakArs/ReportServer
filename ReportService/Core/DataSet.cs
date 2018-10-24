using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Gerakul.ProtoBufSerializer;
using Google.Protobuf.WellKnownTypes;

namespace ReportService.Core
{
    //public class DataSet
    //{
    //    public Dictionary<string, string> ColumnDefinitions;
    //    public List<List<object>> Data;

    //    public MessageDescriptor<DataSet> CreateDescriptor()
    //    {
    //        return MessageDescriptorBuilder.New<DataSet>()
    //            .MessageArray(1, ds => ds.ColumnDefinitions,
    //                (ds, kvp) => ds.ColumnDefinitions.Add(kvp.Key, kvp.Value),
    //                MessageDescriptorBuilder.New<KeyValuePair<string, string>>().CreateDescriptor())
    //            .MessageArray(2, ds=>ds.Data, (ds,list)=>ds.Data.Add(list),
    //                MessageDescriptorBuilder.New<List<object>>().CreateDescriptor())
    //            .CreateDescriptor();
    //    }
    //}

    //public class KeyValuePairStringType
    //{
    //    public string Key;
    //    public string Value;

    //    public KeyValuePairStringType()
    //    {
    //    }

    //    public KeyValuePairStringType(string key, string value)
    //    {
    //        Key = key;
    //        Value = value;
    //    }

    //    public static MessageDescriptor<KeyValuePairStringType> CreateDescriptor()
    //    {
    //        return MessageDescriptorBuilder.New<KeyValuePairStringType>()
    //            .String(1, x => x.Key, (x, y) => x.Key = y)
    //            .String(2, x => x.Value, (x, y) => x.Value = y, x => x.Value != null)
    //            .CreateDescriptor();
    //    }
    //}

    public class SomeEntity : IEquatable<SomeEntity>
    {
        public int IntField;
        public string StringField;
        public double DoubleField;

        public static MessageDescriptor<SomeEntity> GetDescriptor()
        {
            return MessageDescriptorBuilder.New<SomeEntity>()
                .Int32(1, se => se.IntField, (se, intVal) => se.IntField = intVal)
                .String(2, se => se.StringField, (se, strVal) => se.StringField = strVal)
                .Double(3, se => se.DoubleField, (se, doubVal) => se.DoubleField = doubVal)
                .CreateDescriptor();
        }

        public bool Equals(SomeEntity other)
        {
            return other       != null              &&
                   IntField    == other.IntField    &&
                   StringField == other.StringField &&
                   DoubleField == other.DoubleField;
        }
    }


    public class DescriptorInfo
    {
        public int FieldsCount;
        public List<FieldParams> Fields=new List<FieldParams>();

        public static MessageDescriptor<DescriptorInfo> GetDescriptor()
        {
            return MessageDescriptorBuilder.New<DescriptorInfo>()
                .Int32(1, di => di.FieldsCount,
                    (di, count) => di.FieldsCount = count)
                .MessageArray(2, di => di.Fields, (di, fparam) => di.Fields.Add(fparam),
                    FieldParams.GetDescriptor())
                .CreateDescriptor();
        }
    }

    public class FieldParams
    {
        public int Number;
        public string Name;
        public string Type;

        public static MessageDescriptor<FieldParams> GetDescriptor()
        {
            return MessageDescriptorBuilder.New<FieldParams>()
                .Int32(1, fp => fp.Number, (fp, numb) => fp.Number = numb)
                .String(2, fp => fp.Name, (fp, name) => fp.Name = name)
                .String(3, fp => fp.Type, (fp, type) => fp.Type = type)
                .CreateDescriptor();
        }
    }

    public class ElementarySerializer
    {
        public byte[] WriteDescriptor<T>() where T : class
        {
            var innerFields = typeof(T).GetFields();

            var descrInfo = new DescriptorInfo
                {FieldsCount = innerFields.Length};

            for (int i = 0; i < descrInfo.FieldsCount; i++)
            {
                var field = innerFields[i];
                descrInfo.Fields.Add(new FieldParams
                {
                    Name = field.Name,
                    Number = i + 1,
                    Type = field.FieldType.Name
                });
            }

            return DescriptorInfo.GetDescriptor().Write(descrInfo);
        }

        public MessageDescriptor<T> ReadDescriptor<T>(byte[] encodedDescriptor) where T : new()
        {
            var descrInfo = DescriptorInfo.GetDescriptor().Read(encodedDescriptor);

            var builder = MessageDescriptorBuilder.New<T>();

            foreach (var fieldParams in descrInfo.Fields)
            {
                switch (fieldParams.Type)
                {
                    case "Boolean":
                        builder.Bool(fieldParams.Number,
                            tp => Convert.ToBoolean(tp.GetType().GetField(fieldParams.Name)
                                .GetValue(tp)),
                            (tp, val) => tp.GetType().GetField(fieldParams.Name)
                                .SetValue(tp, val));
                        break;
                    case "Double":
                        builder.Double(fieldParams.Number,
                            tp => Convert.ToDouble(tp.GetType().GetField(fieldParams.Name)
                                .GetValue(tp)),
                            (tp, val) => tp.GetType().GetField(fieldParams.Name)
                                .SetValue(tp, val));
                        break;
                    case "String":
                        builder.String(fieldParams.Number,
                            tp => tp.GetType().GetField(fieldParams.Name)
                                .GetValue(tp).ToString(),
                            (tp, val) => tp.GetType().GetField(fieldParams.Name)
                                .SetValue(tp, val));
                        break;
                    case "Int32":
                        builder.Int32(fieldParams.Number,
                            tp => Convert.ToInt32(tp.GetType().GetField(fieldParams.Name)
                                .GetValue(tp)),
                            (tp, val) => tp.GetType().GetField(fieldParams.Name)
                                .SetValue(tp, val));
                        break;
                }
            }

            return builder.CreateDescriptor();
        }


        public byte[][] WriteList<T>(List<T> entities, MessageDescriptor<T> entityDescriptor)
            where T : new()
        {
            var count = entities.Count;

            byte[][] buff = new byte[count][];

            for (int i = 0; i < count; i++)
                buff[i] = entityDescriptor.Write(entities[i]);

            return buff;
        }

        public MessageDescriptor<T> GetDescriptor<T>(byte[] serializedList) where T : new()
        {
            return null;
        }

        public List<T> GetEntities<T>(byte[][] serializedList,
                                      MessageDescriptor<T> entityDescriptor) where T : new()
        {
            return serializedList.Select(entityDescriptor.Read)
                .ToList();
        }
    }
}