using System;
using System.Data.SqlClient;
using System.Text;

namespace ReportService.Operations.Helpers

{
    public class SqlCommandInitializer
    {
        //private static readonly Dictionary<Type, SqlDbType> scalarTypeToDbType = new Dictionary<Type, SqlDbType>()
        //    {
        //        { typeof(Int64),    SqlDbType.BigInt},
        //        { typeof(Boolean),  SqlDbType.Bit},
        //        { typeof(DateTime), SqlDbType.DateTime},
        //        { typeof(Decimal),  SqlDbType.Decimal},
        //        { typeof(Double),   SqlDbType.Float},
        //        { typeof(Int32),    SqlDbType.Int},
        //        { typeof(String),   SqlDbType.NVarChar},
        //        { typeof(Single),   SqlDbType.Real},
        //        { typeof(Int16),    SqlDbType.SmallInt},
        //        { typeof(Byte),     SqlDbType.TinyInt},
        //        { typeof(Guid),     SqlDbType.UniqueIdentifier}
        //    };

        private readonly StringBuilder sqlStringBuilder = new StringBuilder();
        private readonly SqlCommand sqlCommand = new SqlCommand();
        private bool isResolved = false;

        internal void AppendQueryString(string queryString)
        {
            if (isResolved)
                throw new InvalidOperationException("Command was resolved i.e. can`t change it`s query.");
            
            sqlStringBuilder.Append(queryString);
        }
        internal SqlCommand ResolveCommand()
        {
            if (isResolved)
                return sqlCommand;

            sqlCommand.CommandText = sqlStringBuilder.ToString();
            isResolved = true;
            return sqlCommand;
        }
        internal void AddParameterWithValue(string paramName, object paramValue)
        {
            if (isResolved)
                throw new InvalidOperationException("Command was resolved i.e. can`t change it`s params list.");

            sqlCommand.Parameters.AddWithValue(paramName, paramValue);
        }

        internal void HandleClosingBracket()
        {
            if (isResolved)
                throw new InvalidOperationException("Command was resolved i.e. can`t change it`s query.");

            var index = sqlStringBuilder.ToString().LastIndexOf(',');
            if (index >= 0)
                sqlStringBuilder.Remove(index, 1);
            sqlStringBuilder.Append($"){Environment.NewLine}");
        }
    }
}