using GiantTeam.EntityFramework;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace GiantTeam.Postgres
{
    public static class GiantTeamNpgsqlEntityFrameworkExtensions
    {
        public static DbContextOptionsBuilder UseNpgsql(this DbContextOptionsBuilder options, ConnectionOptions connectionOptions)
        {
            NpgsqlConnectionStringBuilder connectionStringBuilder = connectionOptions.ToConnectionStringBuilder();

            NpgsqlConnection connection = new(connectionStringBuilder.ToString());

            if (connectionOptions.CaCertificate is string connectionCaCertificateText)
            {
                connection.ConfigureCaCertificateValidation(connectionCaCertificateText);
            }

            if (connectionOptions.SetRole is not null)
            {
                options.AddInterceptors(new OpenedDbConnectionInterceptor($"SET ROLE {PgQuote.Identifier(connectionOptions.SetRole)};"));
            }

            return options.UseNpgsql(connection);
        }
    }
}
