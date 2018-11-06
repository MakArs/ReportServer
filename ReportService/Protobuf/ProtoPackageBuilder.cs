using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using Google.Protobuf;
using Google.Protobuf.Collections;
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
                    {typeof(byte[]), ScalarType.Bytes}
                };

            ScalarTypesToDotNetTypes =
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

        private VariantValue FillVariantValue(ColumnInfo info, object value)
        {
            var varValue = new VariantValue();

            if (value == null && info.Nullable) //what if is not nullable but null?..
                varValue.IsNull = true;

            // var datefromdtoff = DateTimeOffset.FromUnixTimeSeconds(dtoff).UtcDateTime;

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

        #region ExcelPackageToPackage

        public OperationPackage GetPackage(ExcelPackage excelPackage,
                                           ExcelPackageReadingParameters excelParameters) //todo: logic for maintaining multiple datasets, mb 
        {
            var date = DateTime.Now.ToUniversalTime();

            var queryPackage = new OperationPackage
            {
                Created = ((DateTimeOffset)date).ToUnixTimeSeconds()
            };
            
            var sheet = string.IsNullOrEmpty(excelParameters.SheetName)
                ? excelPackage.Workbook.Worksheets.FirstOrDefault()
                : excelPackage.Workbook.Worksheets.FirstOrDefault(workSheet => workSheet.Name == excelParameters.SheetName);

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
                Columns = { columns },
                Rows = { rows }
            };

            queryPackage.DataSets.Add(set);

            return queryPackage;
        }

        #endregion

        #region ClassEnumToPackage

        public OperationPackage GetPackage<T>(IEnumerable<T> values) where T : class
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

        #endregion

    }
}
