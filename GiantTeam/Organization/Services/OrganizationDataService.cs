using GiantTeam.Postgres;
using GiantTeam.Startup;

namespace GiantTeam.Organization.Services
{
    [IgnoreService]
    public class OrganizationDataService : PgDataService
    {
        public OrganizationDataService(ILogger<OrganizationDataService> logger, string connectionString)
            : base(logger, connectionString)
        {
        }
    }
}
