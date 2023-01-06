using GiantTeam.Postgres;
using Microsoft.EntityFrameworkCore.Diagnostics;
using System.Data.Common;

namespace GiantTeam.EntityFramework
{
    /// <summary>
    /// Executes the provided SQL when the connection is opened.
    /// Useful to run commands like <code>SET ROLE "someotherrole"</code> for example.
    /// </summary>
    public class OpenedDbConnectionInterceptor : DbConnectionInterceptor
    {
        private readonly Sql sql;

        public OpenedDbConnectionInterceptor(FormattableString sql)
        {
            this.sql = Sql.Format(sql);
        }

        public OpenedDbConnectionInterceptor(Sql sql)
        {
            this.sql = sql;
        }

        public override async Task ConnectionOpenedAsync(DbConnection connection, ConnectionEndEventData eventData, CancellationToken cancellationToken = default)
        {
            DbCommand cmd = connection.CreateCommand();
            cmd.CommandText = sql.ToParameterizedSql(out var parameters);
            cmd.Parameters.AddRange(parameters);
            await cmd.ExecuteNonQueryAsync(cancellationToken);
        }

        public override void ConnectionOpened(DbConnection connection, ConnectionEndEventData eventData)
        {
            DbCommand cmd = connection.CreateCommand();
            cmd.CommandText = sql.ToParameterizedSql(out var parameters);
            cmd.Parameters.AddRange(parameters);
            cmd.ExecuteNonQuery();
        }
    }
}
