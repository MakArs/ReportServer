using System.Collections.Generic;
using ReportService.Interfaces.Protobuf;

namespace ReportService.Protobuf
{
    public class DataSet
    {
        public  DataSetParameters dataSetParameters;

        public List<object[]> Rows = new List<object[]>();
    }

    public partial class ElementarySerializer
    {
        public DataSetParameters SaveHeaderFromClassFields<T>() where T : class
        {
            var innerFields = typeof(T).GetFields();

            var descriptor = new DataSetParameters();

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
        public List<DataSetCell> Cells;
    }

    public class DataSetCell
    {

    }
}

//syntax = "proto3";

//message ColumnInfo
//{
//    int32 Tag = 1;
//string Name = 2;
//string TypeName = 3;
//}

//message DataSet
//{
//string Name = 1;
//repeated ColumnInfo = 2;
//repeated Row = 3;
//}

//message Row
//{
//repeated string(?) Values = 1;
//}