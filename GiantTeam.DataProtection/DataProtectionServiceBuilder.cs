using GiantTeam.Postgres;
using GiantTeam.Startup;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System.Security.Cryptography.X509Certificates;
using System.Text.RegularExpressions;

namespace GiantTeam.DataProtection
{
    public class DataProtectionServiceBuilder : IServiceBuilder
    {
        public DataProtectionServiceBuilder(IServiceCollection services, IConfiguration configuration)
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

            string certText;
            if (!string.IsNullOrEmpty(dataProtectionOptions.DataProtectionCertificateFile) &&
                !string.IsNullOrEmpty(dataProtectionOptions.DataProtectionCertificate))
            {
                throw new ApplicationException("Both \"DataProtectionCertificateFile\" and \"DataProtectionCertificate\" are set. Only one can be set at a time.");
            }
            else if (!string.IsNullOrEmpty(dataProtectionOptions.DataProtectionCertificateFile))
            {
                certText = File.ReadAllText(dataProtectionOptions.DataProtectionCertificateFile);
            }
            else if (!string.IsNullOrEmpty(dataProtectionOptions.DataProtectionCertificate))
            {
                certText = dataProtectionOptions.DataProtectionCertificate;
            }
            else
            {
                throw new ApplicationException("Data protection keys must be protected with a certificate.");
            }

            // Unwrap the certificate text
            certText = Regex.Replace(certText, "^-.+", "", RegexOptions.Multiline);
            certText = Regex.Replace(certText, @"\s", "");

            var certBytes = Convert.FromBase64String(certText);

            var certificate = new X509Certificate2(certBytes, string.Empty);

            services
                .AddDataProtection()
                .ProtectKeysWithCertificate(certificate)
                .PersistKeysToDbContext<DataProtectionDbContext>();
        }
    }
}
