using GiantTeam.Postgres;
using GiantTeam.UserManagement.Services;
using Microsoft.Extensions.Options;
using Npgsql;

namespace GiantTeam.Organizations.Services
{
    public class DirectoryDataService : PgDataServiceBase
    {
        private readonly IOptions<GiantTeamOptions> options;
        private readonly SessionService sessionService;
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
    }
}
