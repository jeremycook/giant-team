using Npgsql;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace GiantTeam.Postgres
{
    public static class GiantTeamNpgsqlConnectionExtensions
    {
        /// <summary>
        /// Configures the <see cref="NpgsqlConnection.UserCertificateValidationCallback"/> of <paramref name="connection"/>
        /// against the <paramref name="caCertificateText"/>.
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="configuration"></param>
        public static void ConfigureCaCertificateValidation(this NpgsqlConnection connection, string caCertificateText)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(caCertificateText);
            X509Certificate2 caCertificate = new(bytes);

            connection.UserCertificateValidationCallback = CreateUserCertificateValidationCallback(caCertificate, connection.UserCertificateValidationCallback);
        }

        private static RemoteCertificateValidationCallback CreateUserCertificateValidationCallback(X509Certificate2 caCert, RemoteCertificateValidationCallback? beforeUserCertificateValidationCallback = null)
        {
            return (sender, certificate, chain, sslPolicyErrors) =>
            {
                if (beforeUserCertificateValidationCallback is not null)
                {
                    if (beforeUserCertificateValidationCallback(sender, certificate, chain, sslPolicyErrors) == false)
                    {
                        return false;
                    }
                }

                if (certificate is null)
                {
                    throw new ArgumentNullException(nameof(certificate));
                }

                X509Chain caCertChain = new()
                {
                    ChainPolicy = new X509ChainPolicy()
                    {
                        RevocationMode = X509RevocationMode.NoCheck,
                        RevocationFlag = X509RevocationFlag.EntireChain,
                        ExtraStore = { caCert }
                    }
                };

                X509Certificate2 serverCert = new(certificate);

                caCertChain.Build(serverCert);
                if (caCertChain.ChainStatus.Length == 0)
                {
                    // No errors
                    return true;
                }

                foreach (X509ChainStatus status in caCertChain.ChainStatus)
                {
                    // Check if we got any errors other than UntrustedRoot (which we will always get if we don't install the CA cert to the system store)
                    if (status.Status != X509ChainStatusFlags.UntrustedRoot)
                    {
                        return false;
                    }
                }

                return true;
            };
        }
    }
}
