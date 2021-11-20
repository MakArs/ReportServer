using System.Data.SqlClient;
using System.Threading.Tasks;
using Dapper;
using Microsoft.AspNetCore.Mvc.Testing;

namespace ReportService.Tests
{
    namespace App.API.Tests.Integration
    {
        public class SqlTestDBHelper : WebApplicationFactory<Api.Startup>
        {
            public const string ReportServerTestDbName = "ReportServerTestDb";
            public const string TestDbConnStr = "Server=localhost,1438;User Id=sa;Password=P@55w0rd;Timeout=5";

            public static async Task CreateDB(SqlConnection connection)
            {
                await connection.ExecuteAsync($@"IF NOT EXISTS(SELECT * FROM sys.databases WHERE name = '{ReportServerTestDbName}')
                        BEGIN CREATE DATABASE[{ReportServerTestDbName}] END");
            }
        }
    }
}
