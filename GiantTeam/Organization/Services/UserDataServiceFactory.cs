using GiantTeam.ComponentModel;
using GiantTeam.Postgres;
using GiantTeam.Startup;
using GiantTeam.UserManagement.Services;
using Microsoft.Extensions.Options;
using Npgsql;

namespace GiantTeam.Organization.Services
{
    [Service]
    public class UserDataServiceFactory
    {
        private readonly ILoggerFactory logger;
        private readonly IOptions<GiantTeamOptions> giantTeamOptions;
        private readonly SessionService sessionService;

        public UserDataServiceFactory(
            ILoggerFactory logger,
            IOptions<GiantTeamOptions> giantTeamOptions,
            SessionService sessionService)
        {
            this.logger = logger;
            this.giantTeamOptions = giantTeamOptions;
            this.sessionService = sessionService;
        }

        /// <summary>
        /// Create a new elevated <see cref="PgDataService"/> connection to <paramref name="databaseName"/>.
        /// </summary>
        /// <param name="databaseName"></param>
        public PgDataService NewElevatedDataService(string databaseName, string defaultSchema = "")
        {
            var connectionStringBuilder = new NpgsqlConnectionStringBuilder(giantTeamOptions.Value.UserConnectionString)
            {
                Database = databaseName,
                SearchPath = defaultSchema,
                Username = sessionService.User.DbElevatedLogin ?? throw new UnelevatedException(),
                Password = sessionService.User.DbPassword,
            };

            return new(
                logger: logger.CreateLogger<PgDataService>(),
                connectionString: connectionStringBuilder.ToString());
        }

        /// <summary>
        /// Create a new regular <see cref="PgDataService"/> connection to <paramref name="databaseName"/>.
        /// </summary>
        /// <param name="databaseName"></param>
        public PgDataService NewDataService(string databaseName, string defaultSchema = "")
        {
            var connectionStringBuilder = new NpgsqlConnectionStringBuilder(giantTeamOptions.Value.UserConnectionString)
            {
                Database = databaseName,
                SearchPath = defaultSchema,
                Username = sessionService.User.DbLogin,
                Password = sessionService.User.DbPassword,
            };

            return new(
                logger: logger.CreateLogger<PgDataService>(),
                connectionString: connectionStringBuilder.ToString());
        }
    }
}
