using System;
using System.Collections.Generic;
using System.Linq;
using ReportService.Entities;
using ReportService.Interfaces.Protobuf;

namespace ReportService.Protobuf
{
    public class ProtoPackageParser : IPackageParser
    {
        private object GetFromVariantValue(ColumnInfo info, VariantValue value)
        {
            if (info.Nullable && value.IsNull)
                return null;

            return info.Type switch
            {
                ScalarType.Int32 => (object) value.Int32Value,
                ScalarType.Int16 => value.Int16Value,
                ScalarType.Int8 => value.Int8Value,
                ScalarType.Double => value.DoubleValue,
                ScalarType.Decimal => value.DecimalValue,
                ScalarType.Int64 => value.Int64Value,
                ScalarType.Bool => value.BoolValue,
                ScalarType.String => value.StringValue,
                ScalarType.Bytes => value.BytesValue.ToByteArray(),
                ScalarType.DateTime => DateTimeOffset.FromUnixTimeSeconds(value.DateTimeValue).UtcDateTime,
                ScalarType.DateTimeOffset => DateTimeOffset.FromUnixTimeMilliseconds(value.DateTimeOffsetValue),
                ScalarType.TimeSpan => new TimeSpan(value.TimeSpanValue),
                _ => null
            };
        }

        public List<DataSetContent> GetPackageValues(OperationPackage package)
        {
            var allContent = new List<DataSetContent>();

            foreach (var set in package.DataSets)
            {
                var setHeaders = set.Columns.Select(col => col.Name).ToList();
                var setRows = new List<List<object>>();
                var groupNumbers = set.ViewSettings?.GroupingColumnNumbers?.ToList();
                var orderSettings = set.ViewSettings?.OrderSettings
                    .Select(stgs => new Entities.OrderSettings
                    {
                        ColumnNumber = stgs.ColumnNumber,
                        Descending = stgs.Descending
                    }).ToList();

                foreach (var row in set.Rows)
                {
                    var rowValues = new List<object>();

                    for (int i = 0; i < set.Columns.Count; i++)
                    {
                        var colInfo = set.Columns[i];
                        var varValue = row.Values[i];

                        rowValues.Add(GetFromVariantValue(colInfo, varValue));
                    }

                    setRows.Add(rowValues);
                }

                allContent.Add(new DataSetContent
                {
                    Headers = setHeaders,
                    Rows = setRows,
                    Name = set.Name,
                    GroupColumns = groupNumbers,
                    OrderColumns = orderSettings
                });
            }

            return allContent;
        }
    }
}