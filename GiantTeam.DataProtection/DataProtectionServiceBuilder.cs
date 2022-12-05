using GiantTeam.Postgres;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.RegularExpressions;

namespace GiantTeam.DataProtection
{
    public class DataProtectionServiceBuilder
    {
        public DataProtectionServiceBuilder(IServiceCollection services, IConfiguration configuration, IHostEnvironment environment)
        {
            IConfiguration dataProtectionSection = configuration;
            services.Configure<DataProtectionOptions>(dataProtectionSection);

            services.AddDbContextPool<DataProtectionDbContext>((services, options) =>
            {
                var giantTeamOptions = services.GetRequiredService<IOptions<DataProtectionOptions>>().Value;
                var connectionOptions = giantTeamOptions.DataProtectionConnection;

                options.UseNpgsql(connectionOptions);
            });

            DataProtectionOptions dataProtectionOptions =
                dataProtectionSection.Get<DataProtectionOptions>() ??
                throw new InvalidOperationException();
            if (!string.IsNullOrEmpty(dataProtectionOptions.DataProtectionCertificate))
            {
                // Unwrap the certificate text
                var certText = dataProtectionOptions.DataProtectionCertificate;
                certText = Regex.Replace(certText, "^-.+", "", RegexOptions.Multiline);
                certText = Regex.Replace(certText, @"\s", "");

                var certBytes = Convert.FromBase64String(certText);

                var certificate = new X509Certificate2(certBytes, string.Empty);

                services
                    .AddDataProtection()
                    .ProtectKeysWithCertificate(certificate)
                    .PersistKeysToDbContext<DataProtectionDbContext>();
            }
            else if (environment.IsDevelopment())
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
