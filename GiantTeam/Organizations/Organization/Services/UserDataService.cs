using GiantTeam.Postgres;
using GiantTeam.Startup;
using GiantTeam.UserManagement.Services;
using Microsoft.Extensions.Options;
using Npgsql;

namespace GiantTeam.Organizations.Organization.Services
{
    [IgnoreService]
    public class UserDataService : PgDataServiceBase
    {
        private readonly IOptions<GiantTeamOptions> options;
        private readonly SessionService sessionService;
        private readonly string databaseName;
        private readonly string defaultSchema;
        private string? _connectionString;

        protected override ILogger Logger { get; }

        protected override string ConnectionString
        {
            get
            {
                if (_connectionString is null)
                {
                    var connectionOptions = options.Value.DirectoryConnection;
                    var user = sessionService.User;

                    NpgsqlConnectionStringBuilder connectionStringBuilder = connectionOptions.ToConnectionStringBuilder();
                    connectionStringBuilder.Database = databaseName;
                    connectionStringBuilder.SearchPath = defaultSchema;
                    connectionStringBuilder.Username = user.DbLogin;
                    connectionStringBuilder.Password = user.DbPassword;

                    _connectionString = connectionStringBuilder.ToString();
                }

                return _connectionString;
            }
        }

        public UserDataService(
            ILogger<UserDataService> logger,
            IOptions<GiantTeamOptions> options,
            SessionService sessionService,
            string databaseName,
            string defaultSchema)
        {
            Logger = logger;
            this.options = options;
            this.sessionService = sessionService;
            this.databaseName = databaseName;
            this.defaultSchema = defaultSchema;
        }
    }
}
