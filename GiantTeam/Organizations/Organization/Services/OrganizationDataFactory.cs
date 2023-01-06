using GiantTeam.Organizations.Organization.Data;
using GiantTeam.Postgres;
using GiantTeam.Startup;
using GiantTeam.UserManagement.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Npgsql;

namespace GiantTeam.Organizations.Organization.Services
{
    [Service]
    public class OrganizationDataFactory
    {
        private readonly ILoggerFactory logger;
        private readonly IOptions<GiantTeamOptions> giantTeamOptions;
        private readonly SessionService sessionService;

        public OrganizationDataFactory(
            ILoggerFactory logger,
            IOptions<GiantTeamOptions> giantTeamOptions,
            SessionService sessionService)
        {
            this.logger = logger;
            this.giantTeamOptions = giantTeamOptions;
            this.sessionService = sessionService;
        }

        /// <summary>
        /// Create a new <see cref="OrganizationDataService"/> that connects to <paramref name="databaseName"/>.
        /// </summary>
        /// <param name="databaseName"></param>
        public OrganizationDataService NewDataService(string databaseName, string defaultSchema = "spaces")
        {
            var connectionStringBuilder = new NpgsqlConnectionStringBuilder(giantTeamOptions.Value.OrganizationConnection.ConnectionString)
            {
                Database = databaseName,
                SearchPath = defaultSchema,
                Username = sessionService.User.DbLogin,
                Password = sessionService.User.DbPassword,
            };

            return new(
                logger: logger.CreateLogger<OrganizationDataService>(),
                connectionString: connectionStringBuilder.ToString(),
                untrustedRootCertificate: giantTeamOptions.Value.OrganizationConnection.CaCertificate);
        }

        public OrganizationDbContext NewDbContext(string databaseName)
        {
            var organizationConnection = giantTeamOptions.Value.OrganizationConnection;

            var csb = organizationConnection.CreateConnectionStringBuilder();
            csb.Database = databaseName;
            csb.SearchPath = "spaces";
            csb.Username = sessionService.User.DbLogin;
            csb.Password = sessionService.User.DbPassword;

            var dbContextOptions = new DbContextOptionsBuilder<OrganizationDbContext>()
                .UseSnakeCaseNamingConvention()
                .UseNpgsql(csb, organizationConnection.CaCertificate)
                .Options;

            return new OrganizationDbContext((DbContextOptions<OrganizationDbContext>)dbContextOptions);
        }
    }
}
