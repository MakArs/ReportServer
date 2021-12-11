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
            public const string TestDbConnStr = "Server=localhost,1438;User Id=sa;Password=P@55w0rd;Timeout=5";
            
            public static async Task DropAllConstraintsAndTables(SqlConnection connection)
            {
                await connection.ExecuteAsync(@"declare @sql nvarchar(max) = (
                            select 'alter table ' + quotename(schema_name(schema_id)) + '.' +
                            quotename(object_name(parent_object_id)) +' drop constraint '+quotename(name) + ';'
                            from sys.foreign_keys for xml path('')
                            );
                            exec sp_executesql @sql;
                            EXEC sp_msforeachtable 'drop table ?'");
            }
        }
    }
}
