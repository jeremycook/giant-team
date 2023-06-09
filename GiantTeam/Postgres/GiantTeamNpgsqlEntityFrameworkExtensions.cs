﻿using GiantTeam.EntityFramework;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace GiantTeam.Postgres
{
    public static class GiantTeamNpgsqlEntityFrameworkExtensions
    {
        public static DbContextOptionsBuilder UseNpgsql(
            this DbContextOptionsBuilder options,
            ConnectionOptions connectionOptions)
        {
            return options.UseNpgsql(
                connectionStringBuilder: connectionOptions.ToConnectionStringBuilder(),
                caCertificate: connectionOptions.CaCertificate,
                setRole: connectionOptions.SetRole);
        }

        public static DbContextOptionsBuilder UseNpgsql(
            this DbContextOptionsBuilder options,
            NpgsqlConnectionStringBuilder connectionStringBuilder,
            string? caCertificate = null,
            string? setRole = null)
        {
            NpgsqlConnection connection = new(connectionStringBuilder.ToString());

            if (!string.IsNullOrEmpty(caCertificate))
            {
                connection.ConfigureCaCertificateValidation(caCertificate);
            }
            else if (!string.IsNullOrEmpty(connectionStringBuilder.RootCertificate))
            {
                var certificateBytes = new Span<byte>();
                if (Convert.TryFromBase64String(connectionStringBuilder.RootCertificate, certificateBytes, out _))
                {
                    connection.TrustUserCertificate(certificateBytes);
                }
            }

            if (!string.IsNullOrEmpty(setRole) && connectionStringBuilder.Username != setRole)
            {
                options.AddInterceptors(new OpenedDbConnectionInterceptor($"SET ROLE {Sql.Identifier(setRole)};"));
            }

            return options.UseNpgsql(connection);
        }
    }
}
