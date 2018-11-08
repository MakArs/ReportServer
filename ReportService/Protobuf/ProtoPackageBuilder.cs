using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using Google.Protobuf;
using Google.Protobuf.Collections;
using Google.Protobuf.Reflection;
using OfficeOpenXml;
using ReportService.Extensions;
using ReportService.Interfaces.Protobuf;
using ReportService.Operations.DataImporters;
using Type = System.Type;

namespace ReportService.Protobuf
{
    public class ProtoPackageBuilder : IPackageBuilder
    {
        private readonly Dictionary<ScalarType, Type> ScalarTypesToDotNetTypes;
        private readonly Dictionary<Type, ScalarType> DotNetTypesToScalarTypes;

        public ProtoPackageBuilder()
        {
            DotNetTypesToScalarTypes =
                new Dictionary<Type, ScalarType>
                {
                    {typeof(int), ScalarType.Int32},
                    {typeof(double), ScalarType.Double},
                    {typeof(long), ScalarType.Int64},
                    {typeof(bool), ScalarType.Bool},
                    {typeof(string), ScalarType.String},
                    {typeof(byte[]), ScalarType.Bytes},
                    {typeof(DateTime), ScalarType.DateTime}
                };

            ScalarTypesToDotNetTypes =
                new Dictionary<ScalarType, Type>
                {
                    {ScalarType.Int32, typeof(int)},
                    {ScalarType.Double, typeof(double)},
                    {ScalarType.Int64, typeof(long)},
                    {ScalarType.Bool, typeof(bool)},
                    {ScalarType.String, typeof(string)},
                    {ScalarType.Bytes, typeof(byte[])},
                    {ScalarType.DateTime, typeof(DateTime)},
                    // {ScalarType.TimeStamp, typeof(DateTime)}
                };
        }

        #region DbReaderToPackage

        private RepeatedField<ColumnInfo> GetCurrentResultParameters(DbDataReader reader)
        {
            var columns = new RepeatedField<ColumnInfo>();

            columns.AddRange(reader.GetColumnSchema()
                .Select(column => new ColumnInfo
                {
                    Name = column.ColumnName,
                    Nullable = column.AllowDBNull ?? false,
                    Type = DotNetTypesToScalarTypes[column.DataType]
                })
            );

            return columns;
        }

        private DataSet GetDataSet(DbDataReader reader)
        {
            var columns = GetCurrentResultParameters(reader);

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
                Created = ((DateTimeOffset) date).ToUnixTimeSeconds()
            };

            if (reader.HasRows)
                queryPackage.DataSets.Add(GetDataSet(reader));

            while (reader.NextResult() && reader.HasRows)
                queryPackage.DataSets.Add(GetDataSet(reader));

            for (int i = 0; i < queryPackage.DataSets.Count; i++)
                queryPackage.DataSets[i].Name = $"Dataset{i + 1}";

            return queryPackage;
        }

        #endregion

        #region ExcelPackageToPackage

        public OperationPackage GetPackage(ExcelPackage excelPackage,
                                           ExcelPackageReadingParameters
                                               excelParameters) //todo: logic for maintaining multiple datasets, mb 
        {
            var date = DateTime.Now.ToUniversalTime();

            var queryPackage = new OperationPackage
            {
                Created = ((DateTimeOffset) date).ToUnixTimeSeconds()
            };

            var sheet = string.IsNullOrEmpty(excelParameters.SheetName)
                ? excelPackage.Workbook.Worksheets.FirstOrDefault()
                : excelPackage.Workbook.Worksheets.FirstOrDefault(workSheet =>
                    workSheet.Name == excelParameters.SheetName);

            if (sheet == null) //todo: dataset with error?
                return null;

            var columns = new RepeatedField<ColumnInfo>();

            int firstValueRow;

            int lastValueRow =
                Math.Min(sheet.Cells.Last(cell => !string.IsNullOrEmpty(cell.Text)).End.Row,
                    excelParameters.MaxRowCount);

            if (excelParameters.UseColumnNames)
            {
                firstValueRow = excelParameters.FirstDataRow + 1;

                foreach (var column in excelParameters.ColumnList)
                    columns.Add(new ColumnInfo
                    {
                        Name = sheet
                            .Cells[$"{column}{excelParameters.FirstDataRow}"]
                            .Text,
                        Type = ScalarType.String
                    });
            }

            else
            {
                firstValueRow = excelParameters.FirstDataRow;
                columns.AddRange(excelParameters.ColumnList
                    .Select(col => new ColumnInfo
                    {
                        Name = col,
                        Type = ScalarType.String
                    }));
            }

            var rows = new RepeatedField<Row>();

            for (int i = firstValueRow; i <= lastValueRow; i++)
            {
                var row = new Row();

                foreach (var col in excelParameters.ColumnList)
                    row.Values.Add(new VariantValue
                    {
                        StringValue = sheet.Cells[$"{col}{i}"].Text
                    });

                if (excelParameters.SkipEmptyRows && row.Values
                        .All(val => string.IsNullOrEmpty(val.StringValue)))
                    continue;

                rows.Add(row);
            }

            var set = new DataSet
            {
                Columns = {columns},
                Rows = {rows}
            };

            queryPackage.DataSets.Add(set);

            return queryPackage;
        }

        #endregion

        #region ClassEnumToPackage

        public OperationPackage GetPackage<T>(IEnumerable<T> values) where T : class
        {
            var date = DateTime.Now.ToUniversalTime();
            var type = typeof(T);

            var queryPackage = new OperationPackage
            {
                Created = ((DateTimeOffset) date).ToUnixTimeSeconds(),
                OperationName = type.Name
            };

            var fields = GetClassFields<T>();

            var props = GetClassProps<T>();

            var rows = new RepeatedField<Row>();

            foreach (var value in values)
            {
                var rowValues = new RepeatedField<VariantValue>();

                foreach (var t in fields)
                {
                    rowValues.Add(FillVariantValue(t, type
                        .GetField(t.Name)
                        .GetValue(value)));
                }

                foreach (var t in props)
                {
                    rowValues.Add(FillVariantValue(t, type
                        .GetProperty(t.Name)?
                        .GetValue(value)));
                }

                rows.Add(new Row {Values = {rowValues}});
            }

            var headers = fields.Concat(props);

            var dataSet = new DataSet
            {
                Columns = { headers },
                Name = typeof(T).Name,
                Rows = {rows}
            };
            
            queryPackage.DataSets.Add(dataSet);

            return queryPackage;
        }

        private RepeatedField<ColumnInfo> GetClassFields<T>() where T : class
        {
            var innerFields = typeof(T).GetFields();

            var columns = new RepeatedField<ColumnInfo>();

            foreach (var field in innerFields)
            {
                var type = field.FieldType;
                columns.Add(new ColumnInfo
                {
                    Nullable = !type.IsValueType || Nullable.GetUnderlyingType(type) != null,
                    Name = field.Name,
                    Type = DotNetTypesToScalarTypes[type]
                });
            }

            return columns;
        }

        private RepeatedField<ColumnInfo> GetClassProps<T>() where T : class
        {
            var props = typeof(T).GetProperties();

            var columns = new RepeatedField<ColumnInfo>();

            foreach (var prop in props)
            {
                var type = prop.PropertyType;
                columns.Add(new ColumnInfo
                {
                    Nullable = !type.IsValueType || Nullable.GetUnderlyingType(type) != null,
                    Name = prop.Name,
                    Type = DotNetTypesToScalarTypes[type]
                });
            }

            return columns;
        }

        #endregion

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

                    case ScalarType.DateTime:
                        varValue.DateTime = value is DateTime dateval
                            ? ((DateTimeOffset)dateval).ToUnixTimeSeconds()
                            : 0;
                        break;
                }
            }

            return varValue;
        }

        private object GetFromVariantValue(ColumnInfo info, VariantValue value)
        {
            if (info.Nullable && value.IsNull)
                return null;

            switch (info.Type)
            {
                case ScalarType.Int32:
                    return value.Int32Value;

                case ScalarType.Double:
                    return value.DoubleValue;

                case ScalarType.Int64:
                    return value.Int64Value;

                case ScalarType.Bool:
                    return value.BoolValue;

                case ScalarType.String:
                    return value.StringValue;

                case ScalarType.Bytes:
                    return value.BytesValue.ToByteArray();

                case ScalarType.DateTime:
                    return DateTimeOffset
                        .FromUnixTimeSeconds(value.DateTime).UtcDateTime;
            }

            return null;
        }

        public List<DataSetContent> GetPackageValues(OperationPackage package)
        {
            var allContent = new List<DataSetContent>();

            foreach (var set in package.DataSets)
            {
                var setHeaders = set.Columns.Select(col => col.Name).ToList();
                var setRows = new List<List<object>>();

                foreach (var row in set.Rows)
                {
                    var rowValues = new List<object>();

                    for (int i = 0; i < set.Columns.Count; i++)
                    {
                        var colInfo = set.Columns[i];
                        var varValue = row.Values[i];

                        rowValues.Add(GetFromVariantValue(colInfo,varValue));
                    }

                    setRows.Add(rowValues);
                }

                allContent.Add(new DataSetContent
                {
                    Headers = setHeaders,
                    Rows = setRows,
                    Name = set.Name
                });
            }

            return allContent;
        }
    }
}