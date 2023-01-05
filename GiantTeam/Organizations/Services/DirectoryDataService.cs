using GiantTeam.Postgres;
using GiantTeam.UserManagement.Services;
using Microsoft.Extensions.Options;
using Npgsql;

namespace GiantTeam.Organizations.Services
{
    public class DirectoryDataService : PgDataService
    {
        private readonly IOptions<GiantTeamOptions> options;
        private readonly SessionService sessionService;
        private readonly string? databaseName;
        private readonly string? searchPath;
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
                    connectionStringBuilder.Username = user.DbLogin;
                    connectionStringBuilder.Password = user.DbPassword;

                    if (databaseName is not null)
                    {
                        connectionStringBuilder.Database = databaseName;
                        connectionStringBuilder.SearchPath = searchPath;
                    }

                    _connectionString = connectionStringBuilder.ToString();
                }

                return _connectionString;
            }
        }

        public DirectoryDataService(
            ILogger<DirectoryDataService> logger,
            IOptions<GiantTeamOptions> options,
            SessionService sessionService)
        {
            Logger = logger;
            this.options = options;
            this.sessionService = sessionService;
        }

        private DirectoryDataService(
            ILogger<DirectoryDataService> logger,
            IOptions<GiantTeamOptions> options,
            SessionService sessionService,
            string databaseName,
            string? searchPath)
        {
            Logger = logger;
            this.options = options;
            this.sessionService = sessionService;
            this.databaseName = databaseName;
            this.searchPath = searchPath;
        }

        /// <summary>
        /// Create a new <see cref="DirectoryDataService"/> that connects to <paramref name="databaseName"/>.
        /// </summary>
        /// <param name="databaseName"></param>
        public DirectoryDataService CloneDataService(string databaseName, string? searchPath = null)
        {
            return new((ILogger<DirectoryDataService>)Logger, options, sessionService, databaseName, searchPath);
        }
    }
}
