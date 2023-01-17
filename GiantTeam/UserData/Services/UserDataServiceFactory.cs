using GiantTeam.Cluster.Directory.Services;
using GiantTeam.ComponentModel;
using GiantTeam.Postgres;
using GiantTeam.Startup;
using GiantTeam.UserManagement.Services;
using Microsoft.Extensions.Options;
using Npgsql;

namespace GiantTeam.UserData.Services
{
    [Service]
    public class UserDataServiceFactory
    {
        private readonly ILoggerFactory logger;
        private readonly IOptions<GiantTeamOptions> giantTeamOptions;
        private readonly GetDatabaseNameService getDatabaseNameService;
        private readonly SessionService sessionService;

        public UserDataServiceFactory(
            ILoggerFactory logger,
            IOptions<GiantTeamOptions> giantTeamOptions,
            GetDatabaseNameService getDatabaseNameService,
            SessionService sessionService)
        {
            this.logger = logger;
            this.giantTeamOptions = giantTeamOptions;
            this.getDatabaseNameService = getDatabaseNameService;
            this.sessionService = sessionService;
        }

        /// <summary>
        /// Create a new elevated <see cref="PgDataService"/> connection to <paramref name="organizationId"/>.
        /// </summary>
        /// <param name="organizationId"></param>
        public PgDataService NewElevatedDataService(Guid organizationId, string defaultSchema = "")
        {
            var databaseName = getDatabaseNameService.GetDatabaseName(organizationId);

            var connectionStringBuilder = new NpgsqlConnectionStringBuilder(giantTeamOptions.Value.UserConnectionString)
            {
                Database = databaseName,
                SearchPath = defaultSchema,
                Username = sessionService.User.DbElevatedLogin ?? throw new UnelevatedException(),
                Password = sessionService.User.DbPassword(),
            };

            return new(
                logger: logger.CreateLogger<PgDataService>(),
                connectionString: connectionStringBuilder.ToString());
        }

        /// <summary>
        /// Create a new regular <see cref="PgDataService"/> connection to <paramref name="databaseName"/>.
        /// </summary>
        /// <param name="databaseName"></param>
        public PgDataService NewDataService(Guid organizationId, string defaultSchema = "")
        {
            var databaseName = getDatabaseNameService.GetDatabaseName(organizationId);

            var connectionStringBuilder = new NpgsqlConnectionStringBuilder(giantTeamOptions.Value.UserConnectionString)
            {
                Database = databaseName,
                SearchPath = defaultSchema,
                Username = sessionService.User.DbLogin,
                Password = sessionService.User.DbPassword(),
            };

            return new(
                logger: logger.CreateLogger<PgDataService>(),
                connectionString: connectionStringBuilder.ToString());
        }
    }
}
