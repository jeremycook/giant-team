using Dapper;
using GiantTeam.DatabaseModeling;
using GiantTeam.EntityFramework;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using System.Text;

namespace GiantTeam.Postgres
{
    public static class GiantTeamDevelopmentApplicationExtensions
    {
        public static async Task MigrateDbContextAsync<TDbContext>(this IServiceProvider services, ConnectionOptions migratorConnectionOptions, ConnectionOptions dbContextConnectionOptions)
            where TDbContext : DbContext
        {
            var logger = services.GetRequiredService<ILoggerFactory>().CreateLogger(typeof(GiantTeamDevelopmentApplicationExtensions));
            using var scope = services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<TDbContext>();
            Database database = new();
            EntityFrameworkDatabaseContributor.Singleton.Contribute(database, db.Model);
            if (dbContextConnectionOptions.SetRole is string dbContextRole)
            {
                new DatabaseVisitor(item =>
                {
                    switch (item)
                    {
                        case Schema schema:
                            schema.Privileges.Add(new(dbContextRole, "USAGE"));
                            schema.DefaultPrivileges.Add(new(dbContextRole, DefaultPrivilegesEnum.Tables, "SELECT, INSERT, UPDATE, DELETE"));
                            schema.DefaultPrivileges.Add(new(dbContextRole, DefaultPrivilegesEnum.Sequences, "SELECT, USAGE"));
                            schema.DefaultPrivileges.Add(new(dbContextRole, DefaultPrivilegesEnum.Functions, "EXECUTE"));
                            break;
                    }
                }).Visit(database);
            }
            PgDatabaseScripter scripter = new();

            // Build SQL
            StringBuilder sql = new();
            sql.Append("REVOKE ALL PRIVILEGES ON SCHEMA public FROM PUBLIC;\n\n");
            sql.Append(scripter.Script(database).TrimEnd('\n') + "\n\n");

            // Build connection
            var dbConnectionStringBuilder = dbContextConnectionOptions.ToConnectionStringBuilder();

            // Match the DbContext's host, port and database
            var migratorConnectionStringBuilder = migratorConnectionOptions.ToConnectionStringBuilder();
            migratorConnectionStringBuilder.Host = dbConnectionStringBuilder.Host;
            migratorConnectionStringBuilder.Port = dbConnectionStringBuilder.Port;
            migratorConnectionStringBuilder.Database = dbConnectionStringBuilder.Database;

            var migratorConnection = new NpgsqlConnection(migratorConnectionStringBuilder.ToString());
            if (dbContextConnectionOptions.CaCertificate is not null)
            {
                migratorConnection.ConfigureCaCertificateValidation(dbContextConnectionOptions.CaCertificate);
            }
            await migratorConnection.OpenAsync();
            if (migratorConnectionOptions.SetRole is not null)
            {
                await migratorConnection.SetRoleAsync(migratorConnectionOptions.SetRole);
            }

            // Execute SQL
            string script = sql.ToString();
            logger.LogDebug("Executing migration script of {DbContextType}:\n\n{Script}", typeof(TDbContext).Name, script);
            await migratorConnection.ExecuteAsync(script);
        }
    }
}
