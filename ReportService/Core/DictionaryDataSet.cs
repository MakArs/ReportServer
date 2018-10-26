using System;
using System.Collections.Generic;
using Gerakul.ProtoBufSerializer;

namespace ReportService.Core
{

//    public class DictionaryDataSet
//    {
//       public readonly Dictionary<string, string> ColumnDefinitions = new Dictionary<string, string>();
//        //public List<List<object>> Data;

//        public static MessageDescriptor<DictionaryDataSet> GetDescriptor()
//        {
//            return MessageDescriptorBuilder.New<DictionaryDataSet>()
//                .MessageArray(1, ds => ds.ColumnDefinitions,
//                    (ds, kvp) => ds.ColumnDefinitions.Add(kvp.Key, kvp.Value),
//                    MessageDescriptorBuilder.New<KeyValuePair<string, string>>().CreateDescriptor())
//                //.MessageArray(2, ds => ds.Data,
//                //    (ds, list) => ds.Data.Add(list),
//                //    MessageDescriptorBuilder.New<List<object>>().CreateDescriptor())
//                .CreateDescriptor();
//        }
//    }

//    public partial class ElementarySerializer
//    {
//        public byte[] WriteDictDescriptor<T>() where T : class
//        {
//            var innerFields = typeof(T).GetFields();

//            DictionaryDataSet set=new DictionaryDataSet();

//            foreach (var field in innerFields)
//            {
//                set.ColumnDefinitions.Add(field.Name,field.FieldType.Name);
//            }

//            return DictionaryDataSet.GetDescriptor().Write(set);
//        }

//        public Dictionary<string,string> ReadDictDescriptor(byte[] encodedDescriptor)
//        {
//            var descrInfo = DictionaryDataSet.GetDescriptor().Read(encodedDescriptor);

//            return descrInfo.ColumnDefinitions;
//        }

//        public byte[] WriteDictionary<T>(byte[] encodedDescriptor, T t) where T : class
//        {
//            return null;
//        }

//        public Dictionary<string,object> ReadEntity(byte[] encodedDescriptor, byte[] encodedEntity) 
//        {
//            var dict = ReadDictDescriptor(encodedDictionary);

//            var entity = new object[dict.Count];

//            return null;
//        }
//    }

    public partial class ElementarySerializer
    {
        public List<FieldParams2> SaveHeader<T>() where T : class
        {
            var innerFields = typeof(T).GetFields();

            var fieldpars=new List<FieldParams2>();

            for (int i = 0; i < innerFields.Length; i++)
            {
                var field = innerFields[i];
                fieldpars.Add(new FieldParams2
                {
                    Name = field.Name,
                    Number = i + 1,
                    Type = field.FieldType
                });
            }

            return fieldpars;
        }
    }

    public class Header
    {
        public List<FieldParams2> fprs = new List<FieldParams2>();
    }

    public class FieldParams2
    {
        public int Number;
        public string Name;
        public Type Type;
    }
}