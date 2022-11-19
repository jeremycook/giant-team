using GiantTeam.Postgres;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.Encodings.Web;

namespace GiantTeam.DataProtection
{
    public class DataProtectionServiceBuilder
    {
        public DataProtectionServiceBuilder(IServiceCollection services, IConfiguration configuration, IHostEnvironment environment)
        {
            IConfigurationSection dataProtectionSection = configuration.GetRequiredSection("GiantTeam:DataProtection");
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
            if (dataProtectionOptions.DataProtectionCertificate is not null)
            {
                var bytes = Encoding.UTF8.GetBytes(dataProtectionOptions.DataProtectionCertificate);
                var certificate = new X509Certificate2(bytes);
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
