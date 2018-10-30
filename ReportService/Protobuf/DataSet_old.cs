using System;
using System.Collections.Generic;
using System.Linq;
using Gerakul.ProtoBufSerializer;

namespace ReportService.Protobuf
{
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

        public Dictionary<string, object> ReadObj(byte[] encodedEntity, byte[] encodedDescriptor)
        {
            throw new NotImplementedException();
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
}