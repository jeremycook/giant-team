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
    public class UserDataFactory
    {
        private readonly ILoggerFactory logger;
        private readonly IOptions<GiantTeamOptions> giantTeamOptions;
        private readonly SessionService sessionService;

        public UserDataFactory(
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
            var connectionStringBuilder = new NpgsqlConnectionStringBuilder(giantTeamOptions.Value.UserConnectionString)
            {
                Database = databaseName,
                SearchPath = defaultSchema,
                Username = sessionService.User.DbLogin,
                Password = sessionService.User.DbPassword,
            };

            return new(
                logger: logger.CreateLogger<OrganizationDataService>(),
                connectionString: connectionStringBuilder.ToString());
        }

        /// <summary>
        /// Create a new elevated <see cref="OrganizationDataService"/> that connects to <paramref name="databaseName"/>.
        /// </summary>
        /// <param name="databaseName"></param>
        public OrganizationDataService NewElevatedDataService(string databaseName, string defaultSchema = "spaces")
        {
            var connectionStringBuilder = new NpgsqlConnectionStringBuilder(giantTeamOptions.Value.UserConnectionString)
            {
                Database = databaseName,
                SearchPath = defaultSchema,
                Username = sessionService.User.DbElevatedLogin ?? throw new UnprivilegedException(),
                Password = sessionService.User.DbPassword,
            };

            return new(
                logger: logger.CreateLogger<OrganizationDataService>(),
                connectionString: connectionStringBuilder.ToString());
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
    }
}
