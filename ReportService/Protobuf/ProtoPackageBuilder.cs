using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using Google.Protobuf;
using Google.Protobuf.Collections;
using Google.Protobuf.WellKnownTypes;
using OfficeOpenXml;
using ReportService.Extensions;
using ReportService.Interfaces.Protobuf;
using Type = System.Type;

namespace ReportService.Protobuf
{
    public class ProtoPackageBuilder : IPackageBuilder
    {
        private readonly Dictionary<ScalarType, Type> ScalarTypesToDotNetType;
        private readonly Dictionary<Type, ScalarType> DotNetTypeToScalarType;

        public ProtoPackageBuilder()
        {
            DotNetTypeToScalarType =
                new Dictionary<Type, ScalarType>
                {
                    {typeof(int), ScalarType.Int32},
                    {typeof(double), ScalarType.Double},
                    {typeof(long), ScalarType.Int64},
                    {typeof(bool), ScalarType.Bool},
                    {typeof(string), ScalarType.String},
                    {typeof(byte[]), ScalarType.Bytes}
                };

            ScalarTypesToDotNetType =
                new Dictionary<ScalarType, Type>
                {
                    {ScalarType.Int32, typeof(int)},
                    {ScalarType.Double, typeof(double)},
                    {ScalarType.Int64, typeof(long)},
                    {ScalarType.Bool, typeof(bool)},
                    {ScalarType.String, typeof(string)},
                    {ScalarType.Bytes, typeof(byte[])}
                };
        }

        private RepeatedField<ColumnInfo> GetDbReaderParameters(DbDataReader reader)
        {
            var columns = new RepeatedField<ColumnInfo>();

            columns.AddRange(reader.GetColumnSchema()
                .Select(column => new ColumnInfo
                {
                    Name = column.ColumnName,
                    Nullable = column.AllowDBNull ?? false,
                    Type = DotNetTypeToScalarType[column.DataType]
                })
            );

            return columns;
        }

        private VariantValue FillVariantValue(ColumnInfo info, object value)
        {
            var varValue = new VariantValue();

            if (value == null && info.Nullable) //what if is not nullable but null?..
                varValue.IsNull = true;

            else
            {
                switch (info.Type)
                {
                    case ScalarType.Int32:
                        varValue.Int32Value = value is int intval ? intval : 0;
                        break;

                    case ScalarType.Double:
                        varValue.DoubleValue = value is double doubval ? doubval : 0;
                        break;

                    case ScalarType.Int64:
                        varValue.Int64Value = value is long longval ? longval : 0;
                        break;

                    case ScalarType.Bool:
                        varValue.BoolValue = value is bool boolgval && boolgval;
                        break;

                    case ScalarType.String:
                        varValue.StringValue = value is string stringval ? stringval : "";
                        break;

                    case ScalarType.Bytes:
                        varValue.BytesValue = value is byte[] byteval
                            ? ByteString.CopyFrom(byteval)
                            : ByteString.Empty;
                        break;
                }
            }

            return varValue;
        }

        private DataSet GetDataSet(DbDataReader reader)
        {
            var columns = GetDbReaderParameters(reader);

            var rows = new RepeatedField<Row>();

            while (reader.Read())
            {
                var row = new Row();

                for (int i = 0; i < columns.Count; i++)
                    row.Values.Add(FillVariantValue(columns[i], reader[i]));

                rows.Add(row);
            }

            return new DataSet
            {
                Columns = {columns},
                Rows = {rows}
            };
        }

        public OperationPackage GetPackage(DbDataReader reader)
        {
            var date = DateTime.Now.ToUniversalTime();

            var queryPackage = new OperationPackage
            {
                Created = Timestamp.FromDateTime(date),
                DataSets = { }
            };

            if (reader.HasRows)
                queryPackage.DataSets.Add(GetDataSet(reader));

            while (reader.NextResult()&&reader.HasRows)
                queryPackage.DataSets.Add(GetDataSet(reader));

            for (int i = 0; i < queryPackage.DataSets.Count; i++)
                queryPackage.DataSets[i].Name = $"Dataset{i + 1}";

            return queryPackage;
        }

        public OperationPackage GetPackage<T>(IEnumerable<T> values) where T : class
        {
            throw new System.NotImplementedException();
        }

        public OperationPackage GetPackage(ExcelPackage excelPackage)
        {
            throw new System.NotImplementedException();
        }

        private RepeatedField<ColumnInfo> GetClassParameters<T>() where T : class
        {
            var innerFields = typeof(T).GetFields();

            var columns = new RepeatedField<ColumnInfo>();

            foreach (var field in innerFields)
            {
                columns.Add(new ColumnInfo
                {

                    Name = field.Name,
                    Type = EnumExtensions.EnumHelper
                        .GetEnumValue<ScalarType>(field.FieldType.FullName)
                });
            }

            return columns;
        }
    }
}
