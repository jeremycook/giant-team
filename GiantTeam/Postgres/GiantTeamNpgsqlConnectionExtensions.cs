using Dapper;
using Npgsql;
using System.Net.Security;
using System.Runtime.CompilerServices;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace GiantTeam.Postgres
{
    public static class GiantTeamNpgsqlConnectionExtensions
    {
        public static int SetRole(this NpgsqlConnection connection, string role)
        {
            return connection.Execute($"SET ROLE {PgQuote.Identifier(role)};");
        }

        public static async Task<int> SetRoleAsync(this NpgsqlConnection connection, string role)
        {
            return await connection.ExecuteAsync($"SET ROLE {PgQuote.Identifier(role)};");
        }

        /// <summary>
        /// Configure the <paramref name="builder"/> to trust <see cref="NpgsqlConnectionStringBuilder.RootCertificate"/>
        /// if it is a base64 encoded string that is a certificate. No change is made if it is a file and the file exists.
        /// Throws <see cref="InvalidOperationException"/> if either the file does not exists or the string is not valid base64.
        /// </summary>
        /// <param name="builder"></param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException">If RootCertificate is not a file path to a certificate, a base64 string that is a certificate, or null/empty.</exception>
        public static NpgsqlDataSourceBuilder UseBase64RootCertificateConvention(this NpgsqlDataSourceBuilder builder)
        {
            if (builder.ConnectionStringBuilder.RootCertificate is string rootCert &&
                rootCert != string.Empty)
            {
                Span<byte> certificateBytes = new();
                if (File.Exists(rootCert))
                {
                    // OK
                }
                else if (Convert.TryFromBase64String(rootCert, certificateBytes, out _))
                {
                    builder.TrustUserCertificate(certificateBytes);
                    builder.ConnectionStringBuilder.RootCertificate = null;
                }
                else
                {
                    throw new InvalidOperationException($"The {nameof(NpgsqlConnectionStringBuilder.RootCertificate)} must be a file path to a certificate, a base64 string that is a certificate, or null/empty. If a file, the file was not found. If a base64 string, it was invalid.");
                }
            }

            return builder;
        }

        /// <summary>
        /// Configures the <see cref="NpgsqlDataSourceBuilder.UseUserCertificateValidationCallback(RemoteCertificateValidationCallback)"/> of <paramref name="builder"/>
        /// to trust <paramref name="certificateRawData"/> server connections.
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="configuration"></param>
        public static void TrustUserCertificate(this NpgsqlDataSourceBuilder builder, ReadOnlySpan<byte> certificateRawData)
        {
            X509Certificate2 caCertificate = new(certificateRawData);
            builder.UseUserCertificateValidationCallback(CreateUserRemoteCertificateValidationCallback(caCertificate));
        }

        /// <summary>
        /// Configures the <see cref="NpgsqlConnection.UserCertificateValidationCallback"/> of <paramref name="connection"/>
        /// to trust the certificate contained in <paramref name="caCertificateText"/>.
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="configuration"></param>
        public static void ConfigureCaCertificateValidation(this NpgsqlConnection connection, string caCertificateText)
        {
            byte[] certificateRawData = Encoding.UTF8.GetBytes(caCertificateText);
            TrustUserCertificate(connection, certificateRawData);
        }

        /// <summary>
        /// Configures the <see cref="NpgsqlConnection.UserCertificateValidationCallback"/> of <paramref name="connection"/>
        /// to trust the certificate contained in <paramref name="rootCertificateRawData"/>.
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="rootCertificateRawData"></param>
        public static void TrustUserCertificate(this NpgsqlConnection connection, ReadOnlySpan<byte> rootCertificateRawData)
        {
            X509Certificate2 caCertificate = new(rootCertificateRawData);
            connection.UserCertificateValidationCallback = CreateUserRemoteCertificateValidationCallback(caCertificate);
        }

        private static RemoteCertificateValidationCallback CreateUserRemoteCertificateValidationCallback(X509Certificate2 caCert)
        {
            return (sender, certificate, chain, sslPolicyErrors) =>
            {
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
