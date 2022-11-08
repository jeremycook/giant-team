using GiantTeam.EntityFramework;
using GiantTeam.Postgres;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Npgsql;
using System.Security.Cryptography.X509Certificates;

namespace GiantTeam.DataProtection
{
    public class DataProtectionServiceBuilder
    {
        public DataProtectionServiceBuilder(IServiceCollection services, IConfiguration Configuration, IHostEnvironment Environment)
        {
            DataProtectionOptions dataProtectionOptions = Configuration
                .GetRequiredSection("DataProtection")
                .Get<DataProtectionOptions>() ??
                throw new InvalidOperationException();

            string connectionString = dataProtectionOptions.ConnectionString;
            string? connectionCaCertificate = dataProtectionOptions.ConnectionCaCertificate;
            string? setRole = dataProtectionOptions.SetRole;

            services.AddDbContext<DataProtectionDbContext>(options =>
            {
                NpgsqlConnection connection = new(connectionString);

                if (connectionCaCertificate is not null)
                {
                    connection.ConfigureCaCertificateValidation(connectionCaCertificate);
                }

                if (setRole is not null)
                {
                    options.AddInterceptors(new OpenedDbConnectionInterceptor($"SET ROLE {PgQuote.Identifier(setRole)};"));

                }

                options.UseNpgsql(connection);
            });
            if (dataProtectionOptions.ProtectionCertificate is not null)
            {
                var certificate = new X509Certificate2(dataProtectionOptions.ProtectionCertificate);
                services
                    .AddDataProtection()
                    .ProtectKeysWithCertificate(certificate)
                    .PersistKeysToDbContext<DataProtectionDbContext>();
            }
            else if (Environment.IsDevelopment())
            {
                // Keys can be persisted unencrypted in development
                services
                    .AddDataProtection()
                    .PersistKeysToDbContext<DataProtectionDbContext>();
            }
            else
            {
                throw new ApplicationException("Data protection keys must be encrypted at rest in non-development environments.");
            }
        }
    }
}
