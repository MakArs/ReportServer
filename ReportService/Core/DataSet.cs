using System;
using System.Collections.Generic;
using System.Linq;
using Gerakul.ProtoBufSerializer;

namespace ReportService.Core
{
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

    //    public static MessageDescriptor<KeyValuePairStringType> GetDescriptor()
    //    {
    //        return MessageDescriptorBuilder.New<KeyValuePairStringType>()
    //            .String(1, x => x.Key, (x, y) => x.Key = y)
    //            .String(2, x => x.Value, (x, y) => x.Value = y, x => x.Value != null)
    //            .CreateDescriptor();
    //    }
    //}

    //public class SomeEntity : IEquatable<SomeEntity>
    //{
    //    public int IntField;
    //    public string StringField;
    //    public double DoubleField;

    //    public static MessageDescriptor<SomeEntity> GetDescriptor()
    //    {
    //        return MessageDescriptorBuilder.New<SomeEntity>()
    //            .Int32(1, se => se.IntField, (se, intVal) => se.IntField = intVal)
    //            .String(2, se => se.StringField, (se, strVal) => se.StringField = strVal)
    //            .Double(3, se => se.DoubleField, (se, doubVal) => se.DoubleField = doubVal)
    //            .CreateDescriptor();
    //    }

    //    public bool Equals(SomeEntity other)
    //    {
    //        return other       != null              &&
    //               IntField    == other.IntField    &&
    //               StringField == other.StringField &&
    //               DoubleField == other.DoubleField;
    //    }
    //}

    public class DescriptorInfo
    {
        public int FieldsCount;
        public List<FieldParams> Fields = new List<FieldParams>();

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

    public partial class ElementarySerializer
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
                            obj => Convert.ToBoolean(obj.GetType().GetField(fieldParams.Name)
                                .GetValue(obj)),
                            (obj, val) => obj.GetType().GetField(fieldParams.Name)
                                .SetValue(obj, val));
                        break;

                    case "Double":
                        builder.Double(fieldParams.Number,
                            obj => Convert.ToDouble(obj.GetType().GetField(fieldParams.Name)
                                .GetValue(obj)),
                            (obj, val) => obj.GetType().GetField(fieldParams.Name)
                                .SetValue(obj, val));
                        break;

                    case "String":
                        builder.String(fieldParams.Number,
                            obj => obj.GetType().GetField(fieldParams.Name)
                                .GetValue(obj).ToString(),
                            (obj, val) => obj.GetType().GetField(fieldParams.Name)
                                .SetValue(obj, val));
                        break;

                    case "Int32":
                        builder.Int32(fieldParams.Number,
                            obj => Convert.ToInt32(obj.GetType().GetField(fieldParams.Name)
                                .GetValue(obj)),
                            (obj, val) => obj.GetType().GetField(fieldParams.Name)
                                .SetValue(obj, val));
                        break;
                }
            }

            return builder.CreateDescriptor();
        }

       public IUntypedMessageDescriptor ReadDescriptor(byte[] encodedDescriptor)
        {
            var descrInfo = DescriptorInfo.GetDescriptor().Read(encodedDescriptor);

            var builder = MessageDescriptorBuilder.New<object>();

            foreach (var fieldParams in descrInfo.Fields)
            {
                switch (fieldParams.Type)
                {
                    case "Boolean":
                        builder.Bool(fieldParams.Number,
                            obj => Convert.ToBoolean(obj.GetType().GetField(fieldParams.Name)
                                .GetValue(obj)),
                            (obj, val) => obj.GetType().GetField(fieldParams.Name)
                                .SetValue(obj, val));
                        break;

                    case "Double":
                        builder.Double(fieldParams.Number,
                            obj => Convert.ToDouble(obj.GetType().GetField(fieldParams.Name)
                                .GetValue(obj)),
                            (obj, val) => obj.GetType().GetField(fieldParams.Name)
                                .SetValue(obj, val));
                        break;

                    case "String":
                        builder.String(fieldParams.Number,
                            obj => obj.GetType().GetField(fieldParams.Name)
                                .GetValue(obj).ToString(),
                            (obj, val) => obj.GetType().GetField(fieldParams.Name)
                                .SetValue(obj, val));
                        break;

                    case "Int32":
                        builder.Int32(fieldParams.Number,
                            obj => Convert.ToInt32(obj.GetType().GetField(fieldParams.Name)
                                .GetValue(obj)),
                            (obj, val) => obj.GetType().GetField(fieldParams.Name)
                                .SetValue(obj, val));
                        break;
                }
            }

            return builder.CreateDescriptor();
        }

        public byte[] WriteObj<T>(T entity, byte[] encodedDescr)
        {
            var descr = ReadDescriptor(encodedDescr);
            return descr.Write(entity);
        }

        public Dictionary<string,object> ReadObj(byte[] encodedEntity, byte[] encodedDescriptor)
        {
            //var descrInfo = DescriptorInfo.GetDescriptor().Read(encodedDescriptor);
            //var dictdescr= MessageDescriptorBuilder.New<Dictionary<string,object>>()
            //    .array
            //    MessageDescriptor
            //return null;
            var descrInfo = DescriptorInfo.GetDescriptor().Read(encodedDescriptor);

            descrInfo.Fields.ToDictionary(field => field.Name, field => new object());

          var customDescr = MessageDescriptorBuilder.New<Dictionary<string, object>>()
                .MessageArray(1, dict => dict.Select(row => new DataSetRow(row.Key, row.Value)),
                    (dict,newVal)=>dict.Add(newVal.Key,newVal.Value),
                    DataSetRow.GetDescriptor("String"))
                .CreateDescriptor();


            var descr = ReadDescriptor(encodedDescriptor);
            return customDescr.Read(encodedEntity);
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

        public List<T> GetEntities<T>(byte[][] serializedList,
                                      MessageDescriptor<T> entityDescriptor) where T : new()
        {
            return serializedList.Select(entityDescriptor.Read)
                .ToList();
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