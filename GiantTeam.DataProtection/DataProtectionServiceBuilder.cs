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

            services.AddDbContext<DataProtectionDbContext>(options =>
            {
                if (dataProtectionOptions.Connection.SetRole is not null)
                {
                    options.AddInterceptors(new OpenedDbConnectionInterceptor($"SET ROLE {PgQuote.Identifier(dataProtectionOptions.Connection.SetRole)};"));
                }

                NpgsqlConnectionStringBuilder connectionStringBuilder = new(dataProtectionOptions.Connection.ConnectionString);
                if (dataProtectionOptions.Connection.Password is not null)
                {
                    connectionStringBuilder.Password = dataProtectionOptions.Connection.Password;
                }

                NpgsqlConnection connection = new(connectionStringBuilder.ToString());

                if (dataProtectionOptions.Connection.CaCertificate is not null)
                {
                    connection.ConfigureCaCertificateValidation(dataProtectionOptions.Connection.CaCertificate);
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
