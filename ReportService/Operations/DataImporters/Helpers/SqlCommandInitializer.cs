using System;
using System.Data.SqlClient;
using System.Text;

namespace ReportService.Operations.DataImporters.Helpers

{
    public class SqlCommandInitializer
    {
        private readonly StringBuilder mSqlStringBuilder = new StringBuilder();
        private readonly SqlCommand mSqlCommand = new SqlCommand();
        private bool mIsResolved;

        internal void AppendQueryString(string queryString)
        {
            if (mIsResolved)
                throw new InvalidOperationException("Command was resolved i.e. can`t change it`s query.");
            
            mSqlStringBuilder.Append(queryString);
        }

        public SqlCommand ResolveCommand()
        {
            if (mIsResolved)
                return mSqlCommand;

            mSqlCommand.CommandText = mSqlStringBuilder.ToString();
            mIsResolved = true;

            return mSqlCommand;
        }

        internal void AddParameterWithValue(string paramName, object paramValue)
        {
            if (mIsResolved)
                throw new InvalidOperationException("Command was resolved i.e. can`t change it`s params list.");

            mSqlCommand.Parameters.AddWithValue(paramName, paramValue);
        }

        internal void HandleClosingBracket()
        {
            if (mIsResolved)
                throw new InvalidOperationException("Command was resolved i.e. can`t change it`s query.");

            var index = mSqlStringBuilder.ToString().LastIndexOf(',');
            if (index >= 0)
                mSqlStringBuilder.Remove(index, 1);

            mSqlStringBuilder.Append($"){Environment.NewLine}");
        }
    }
}
