using GiantTeam.EntityFramework;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace GiantTeam.Postgres
{
    public static class GiantTeamNpgsqlEntityFrameworkExtensions
    {
        public static DbContextOptionsBuilder UseNpgsql(this DbContextOptionsBuilder options, BasicConnectionOptions connectionOptions)
        {
            NpgsqlConnectionStringBuilder connectionStringBuilder = new(connectionOptions.ConnectionString);

            if (connectionOptions is ExtendedConnectionOptions extendedConnectionOptions)
            {
                if (extendedConnectionOptions.SetRole is not null)
                {
                    options.AddInterceptors(new OpenedDbConnectionInterceptor($"SET ROLE {PgQuote.Identifier(extendedConnectionOptions.SetRole)};"));
                }

                if (extendedConnectionOptions.Password is not null)
                {
                    connectionStringBuilder.Password = extendedConnectionOptions.Password;
                }
            }

            NpgsqlConnection connection = new(connectionStringBuilder.ToString());

            if (connectionOptions.CaCertificate is string connectionCaCertificateText)
            {
                connection.ConfigureCaCertificateValidation(connectionCaCertificateText);
            }

            return options.UseNpgsql(connection);
        }
    }
}
