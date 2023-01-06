using GiantTeam.Postgres;
using GiantTeam.Startup;

namespace GiantTeam.Organizations.Organization.Services
{
    [IgnoreService]
    public class OrganizationDataService : PgDataService
    {
        public OrganizationDataService(
            ILogger logger,
            string connectionString,
            string? untrustedRootCertificate = null,
            string? setRole = null)
            : base(logger, connectionString, untrustedRootCertificate, setRole)
        {
        }
    }
}
