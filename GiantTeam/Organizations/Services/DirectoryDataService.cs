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
        private string? _connectionString;

        public override string ConnectionString
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
            IOptions<GiantTeamOptions> options,
            SessionService sessionService)
        {
            this.options = options;
            this.sessionService = sessionService;
        }
    }
}
