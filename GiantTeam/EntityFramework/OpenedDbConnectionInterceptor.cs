using Microsoft.EntityFrameworkCore.Diagnostics;
using System.Data.Common;

namespace GiantTeam.EntityFramework
{
    /// <summary>
    /// Executes the provided SQL when the connection is opened.
    /// Useful to run commands like <code>SET ROLE SOMEOTHERROLE;</code> for example.
    /// </summary>
    public class OpenedDbConnectionInterceptor : DbConnectionInterceptor
    {
        private readonly string sql;

        public OpenedDbConnectionInterceptor(string sql)
        {
            this.sql = sql;
        }

        public override async Task ConnectionOpenedAsync(DbConnection connection, ConnectionEndEventData eventData, CancellationToken cancellationToken = default)
        {
            DbCommand cmd = connection.CreateCommand();
            cmd.CommandText = sql;
            await cmd.ExecuteNonQueryAsync(cancellationToken);
        }

        public override void ConnectionOpened(DbConnection connection, ConnectionEndEventData eventData)
        {
            DbCommand cmd = connection.CreateCommand();
            cmd.CommandText = sql;
            cmd.ExecuteNonQuery();
        }
    }
}
