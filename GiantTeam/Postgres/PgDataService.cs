using GiantTeam.Organizations.Organization.Services;
using GiantTeam.Startup;
using Npgsql;

namespace GiantTeam.Postgres
{
    [IgnoreService]
    public class PgDataService : PgDataServiceBase
    {
        protected override ILogger Logger { get; }
        protected override string ConnectionString { get; }
        protected string? UntrustedRootCertificate { get; }
        // TODO: SetRole
        protected string? SetRole { get; }

        public PgDataService(
            ILogger logger,
            string connectionString,
            string? untrustedRootCertificate = null,
            string? setRole = null)
        {
            Logger = logger;
            ConnectionString = connectionString;
            UntrustedRootCertificate = untrustedRootCertificate;
            SetRole = setRole;
        }

        public override NpgsqlDataSource CreateDataSource()
        {
            var builder = new NpgsqlDataSourceBuilder(ConnectionString);
            if (UntrustedRootCertificate is not null)
            {
                builder.UseUntrustedRootCertificateValidation(UntrustedRootCertificate);
            }
            return builder.Build();
        }
    }
}
