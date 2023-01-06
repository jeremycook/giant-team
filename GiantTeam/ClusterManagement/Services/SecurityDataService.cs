using GiantTeam.Postgres;
using Microsoft.Extensions.Options;

namespace GiantTeam.ClusterManagement.Services
{
    public class SecurityDataService : PgDataService
    {
        public SecurityDataService(ILogger<SecurityDataService> logger, IOptions<GiantTeamOptions> options)
            : base(logger, options.Value.SecurityManagerConnection.CreateConnectionStringBuilder().ConnectionString)
        {
        }
    }
}
