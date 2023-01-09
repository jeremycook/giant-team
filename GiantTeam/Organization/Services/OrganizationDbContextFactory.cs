using GiantTeam.ComponentModel;
using GiantTeam.Organization.Data;
using GiantTeam.Postgres;
using GiantTeam.Startup;
using GiantTeam.UserManagement.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Npgsql;

namespace GiantTeam.Organization.Services
{
    [Service]
    public class OrganizationDbContextFactory
    {
        private readonly IOptions<GiantTeamOptions> giantTeamOptions;
        private readonly SessionService sessionService;

        public OrganizationDbContextFactory(
            IOptions<GiantTeamOptions> giantTeamOptions,
            SessionService sessionService)
        {
            this.giantTeamOptions = giantTeamOptions;
            this.sessionService = sessionService;
        }

        public OrganizationDbContext NewDbContext(string databaseName)
        {
            var connectionStringBuilder = new NpgsqlConnectionStringBuilder(giantTeamOptions.Value.UserConnectionString)
            {
                Database = databaseName,
                SearchPath = "spaces",
                Username = sessionService.User.DbLogin,
                Password = sessionService.User.DbPassword
            };

            var dbContextOptions = new DbContextOptionsBuilder<OrganizationDbContext>()
                .UseSnakeCaseNamingConvention()
                .UseNpgsql(connectionStringBuilder)
                .Options;

            return new OrganizationDbContext((DbContextOptions<OrganizationDbContext>)dbContextOptions);
        }

        public OrganizationDbContext NewElevatedDbContext(string databaseName)
        {
            var connectionStringBuilder = new NpgsqlConnectionStringBuilder(giantTeamOptions.Value.UserConnectionString)
            {
                Database = databaseName,
                SearchPath = "spaces",
                Username = sessionService.User.DbElevatedLogin ?? throw new UnelevatedException(),
                Password = sessionService.User.DbPassword
            };

            var dbContextOptions = new DbContextOptionsBuilder<OrganizationDbContext>()
                .UseSnakeCaseNamingConvention()
                .UseNpgsql(connectionStringBuilder)
                .Options;

            return new OrganizationDbContext((DbContextOptions<OrganizationDbContext>)dbContextOptions);
        }
    }
}
