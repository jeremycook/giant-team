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
            var dataProtectionOptions = Configuration
                .GetRequiredSection("DataProtection")
                .Get<DataProtectionOptions>();

            services.AddDbContext<DataProtectionDbContext>(options =>
            {
                string connectionString = dataProtectionOptions.ConnectionString;
                NpgsqlConnection connection = new(connectionString);

                if (dataProtectionOptions.ConnectionCaCertificate is not null)
                {
                    connection.ConfigureCaCertificateValidation(dataProtectionOptions.ConnectionCaCertificate);
                }

                options
                .AddInterceptors(new OpenedDbConnectionInterceptor($"SET ROLE {PgQuote.Identifier("giantteam")};"))
                .UseNpgsql(connection);
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
                services
                    .AddDataProtection()
                    .PersistKeysToDbContext<DataProtectionDbContext>();
            }
            else
            {
                throw new ApplicationException("Data protection must be encrypted at rest under non-development environments.");
            }
        }
    }
}
