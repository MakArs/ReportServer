using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using CsvHelper;
using Google.Protobuf;
using Google.Protobuf.Collections;
using OfficeOpenXml;
using ReportService.Interfaces.Protobuf;
using ReportService.Operations.DataImporters;

namespace ReportService.Protobuf
{
    public class ProtoPackageBuilder : IPackageBuilder
    {

        private readonly Dictionary<Type, ScalarType> dotNetTypesToScalarTypes;

        public ProtoPackageBuilder()
        {
            dotNetTypesToScalarTypes =
                new Dictionary<Type, ScalarType>
                {
                    {typeof(int), ScalarType.Int32},
                    {typeof(double), ScalarType.Double},
                    {typeof(long), ScalarType.Int64},
                    {typeof(bool), ScalarType.Bool},
                    {typeof(string), ScalarType.String},
                    {typeof(byte[]), ScalarType.Bytes},
                    {typeof(decimal), ScalarType.Decimal},
                    {typeof(DateTime), ScalarType.DateTime},
                    {typeof(DateTimeOffset), ScalarType.DateTimeOffset},
                    {typeof(char[]), ScalarType.String},
                    {typeof(short), ScalarType.Int16},
                    {typeof(TimeSpan), ScalarType.TimeSpan}, //
                    {typeof(byte), ScalarType.Int8}
                };
        }

        #region DbReaderToPackage

        private T GetDbColumnValue<T>(DataColumnCollection schemaColumns,
            DataRow schemaRow, string columnName)
        {
            if (!schemaColumns.Contains(columnName))
                return default;

            object obj = schemaRow[columnName];

            if (obj is T variable)
                return variable;

            return default;
        }

        private RepeatedField<ColumnInfo> GetCurrentResultParameters(DbDataReader reader)
        {
            var columns = new RepeatedField<ColumnInfo>();

            var schemaTable = reader.GetSchemaTable();
            if (schemaTable == null) return null;

            var cols = schemaTable.Columns;
            var rows = schemaTable.Rows;

            foreach (DataRow row in (InternalDataCollectionBase) rows)
            {
                columns.Add(new ColumnInfo
                {
                    Name = GetDbColumnValue<string>(cols, row, SchemaTableColumn.ColumnName),
                    Nullable = GetDbColumnValue<bool?>(cols, row, SchemaTableColumn.AllowDBNull) ?? false,
                    Type = dotNetTypesToScalarTypes[GetDbColumnValue<Type>(cols, row, SchemaTableColumn.DataType)]
                });
            }

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
                {
                    var value = reader.IsDBNull(i) ? null : reader[i];
                    row.Values.Add(FillVariantValue(columns[i], value));
                }

                rows.Add(row);
            }

            return new DataSet
            {
                Columns = {columns},
                Rows = {rows}
            };
        }

        public OperationPackage GetPackage(DbDataReader reader, string groupNumbers)
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

            if (!string.IsNullOrEmpty(groupNumbers))
                queryPackage.DataSets.First().ViewSettings = FillViewSettings(groupNumbers);

            for (int i = 0; i < queryPackage.DataSets.Count; i++)
                if (string.IsNullOrEmpty(queryPackage.DataSets[i].Name))
                    queryPackage.DataSets[i].Name = $"Dataset{i + 1}";

            return queryPackage;
        }

        #endregion

        #region ExcelPackageToPackage

        public OperationPackage GetPackage(ExcelPackage excelPackage,
            ExcelPackageReadingParameters
                excelParameters, string groupNumbers) //todo: logic for maintaining multiple datasets, mb 
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

                if (excelParameters.ColumnList.Length > 0)
                    foreach (var column in excelParameters.ColumnList)
                        columns.Add(new ColumnInfo
                        {
                            Name = sheet
                                .Cells[$"{column}{excelParameters.FirstDataRow}"]
                                .Text,
                            Type = ScalarType.String
                        });

                else
                {
                    var lastcol = sheet.Cells.End.Column;
                    var firstcol = sheet.Cells.Start.Column;
                    for (int i = firstcol; i < lastcol + 1; i++)
                    {
                        columns.Add(new ColumnInfo
                        {
                            Name = sheet
                                .Cells[$"{i}{excelParameters.FirstDataRow}"]
                                .Text,
                            Type = ScalarType.String
                        });
                    }
                }
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
                Name = sheet.Name,
                Columns = {columns},
                Rows = {rows},
                ViewSettings = FillViewSettings(groupNumbers)
            };

            queryPackage.DataSets.Add(set);

            for (int i = 0; i < queryPackage.DataSets.Count; i++)
                if (string.IsNullOrEmpty(queryPackage.DataSets[i].Name))
                    queryPackage.DataSets[i].Name = $"Dataset{i + 1}";

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
                Columns = {headers},
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
                    Type = dotNetTypesToScalarTypes[type]
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
                    Type = dotNetTypesToScalarTypes[type]
                });
            }

            return columns;
        }

        #endregion

        #region CsvReaderToPackage

        public OperationPackage
            GetPackage(CsvReader reader,
                string groupNumbers) //todo: logic for maintaining multiple datasets, mb 
        {
            var date = DateTime.Now.ToUniversalTime();

            var queryPackage = new OperationPackage
            {
                Created = ((DateTimeOffset) date).ToUnixTimeSeconds()
            };

            if (!reader.Read())
                return queryPackage;

            var columns = new RepeatedField<ColumnInfo>();

            reader.ReadHeader();
            var headers = reader.Context.HeaderRecord;

            foreach (var header in headers)
                columns.Add(new ColumnInfo
                {
                    Name = header,
                    Type = ScalarType.String
                });

            var rows = new RepeatedField<Row>();

            while (reader.Read())
            {
                var row = new Row();

                foreach (var record in reader.Context.Record)
                    row.Values.Add(new VariantValue
                    {
                        StringValue = record
                    });

                rows.Add(row);
            }

            var set = new DataSet
            {
                Name = "Dataset1",
                Columns = {columns},
                Rows = {rows},
                ViewSettings = FillViewSettings(groupNumbers)
            };

            queryPackage.DataSets.Add(set);

            return queryPackage;
        }

        #endregion

        private ViewSettings FillViewSettings(string groupNumbers)
        {
            if (string.IsNullOrEmpty(groupNumbers))
                return null;

            var groupNumbersList = groupNumbers.Split(new[] {';'},
                    StringSplitOptions.RemoveEmptyEntries)
                .Where(name => !string.IsNullOrWhiteSpace(name))
                .Select(int.Parse);

            var viewSettings = new ViewSettings
            {
                GroupingColumnNumbers = {groupNumbersList}
            };

            return viewSettings;
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
                        varValue.Int32Value = value is int intval
                            ? intval
                            : 0;
                        break;

                    case ScalarType.Int16:
                        varValue.Int16Value = value is short shortval
                            ? shortval
                            : 0;
                        break;

                    case ScalarType.Int8:
                        varValue.Int8Value = value is byte tinyval ? tinyval : 0;
                        break;

                    case ScalarType.Double:
                        varValue.DoubleValue = value is double doubval
                            ? doubval
                            : 0;
                        break;

                    case ScalarType.Decimal:
                        varValue.DecimalValue = value is decimal decval ? (double) decval : 0;
                        break;

                    case ScalarType.Int64:
                        varValue.Int64Value = value is long longval ? longval : 0;
                        break;

                    case ScalarType.Bool:
                        varValue.BoolValue = value is bool boolgval && boolgval;
                        break;

                    case ScalarType.String:
                        varValue.StringValue = value is string stringval ? stringval
                            : value is char[] charval ? string.Concat(charval)
                            : "";
                        break;

                    case ScalarType.Bytes:
                        varValue.BytesValue = value is byte[] byteval
                            ? ByteString.CopyFrom(byteval)
                            : ByteString.Empty;
                        break;

                    case ScalarType.DateTime:
                        varValue.DateTimeValue = value is DateTime dateval
                            ? ((DateTimeOffset) dateval.Add(dateval - dateval.ToUniversalTime()))
                            .ToUnixTimeSeconds() //fix saving utc datetime
                            : 0;
                        break;

                    case ScalarType.DateTimeOffset:
                        varValue.DateTimeOffsetValue = value is DateTimeOffset offset
                            ? offset.ToUnixTimeMilliseconds()
                            : 0;
                        break;

                    case ScalarType.TimeSpan:
                        varValue.TimeSpanValue = value is TimeSpan span
                            ? span.Ticks
                            : 0;
                        break;

                    //case ScalarType.TimeStamp:
                    //    varValue.TimeStamp = value is TimeSpan span
                    //        ? new Timestamp {Seconds = span.Seconds}
                    //        : new Timestamp();
                    //    break;
                }
            }

            return varValue;
        }
    }
}