using GiantTeam.Postgres;
using GiantTeam.Services;
using Microsoft.Extensions.Options;
using Npgsql;

namespace GiantTeam.WorkspaceInteraction.Services
{
    public class DatabaseConnectionService
    {
        private readonly IOptions<GiantTeamOptions> options;
        private readonly SessionService sessionService;

        public DatabaseConnectionService(IOptions<GiantTeamOptions> options, SessionService sessionService)
        {
            this.options = options;
            this.sessionService = sessionService;
        }

        public async Task<NpgsqlConnection> OpenAdminConnectionAsync(string database)
        {
            SessionUser user = sessionService.User;
            var workspaceConnection = options.Value.WorkspaceConnection;

            NpgsqlConnectionStringBuilder connectionStringBuilder = workspaceConnection.ToConnectionStringBuilder();
            connectionStringBuilder.Database = database;
            connectionStringBuilder.Username = user.DbLogin;
            connectionStringBuilder.Password = user.DbPassword;

            NpgsqlConnection connection = new(connectionStringBuilder.ToString());

            if (workspaceConnection.CaCertificate is not null)
            {
                connection.ConfigureCaCertificateValidation(workspaceConnection.CaCertificate);
            }

            await connection.OpenAsync();
            await connection.SetRoleAsync(database);
            return connection;
        }

        public async Task<NpgsqlConnection> OpenUserConnectionAsync(string database)
        {
            SessionUser user = sessionService.User;
            var workspaceConnection = options.Value.WorkspaceConnection;

            NpgsqlConnectionStringBuilder connectionStringBuilder = workspaceConnection.ToConnectionStringBuilder();
            connectionStringBuilder.Database = database;
            connectionStringBuilder.Username = user.DbLogin;
            connectionStringBuilder.Password = user.DbPassword;

            NpgsqlConnection connection = new(connectionStringBuilder.ToString());

            if (workspaceConnection.CaCertificate is not null)
            {
                connection.ConfigureCaCertificateValidation(workspaceConnection.CaCertificate);
            }

            await connection.OpenAsync();
            await connection.SetRoleAsync(user.DbRole);
            return connection;
        }

        public NpgsqlConnection CreateUserConnection(string database)
        {
            SessionUser user = sessionService.User;
            var workspaceConnection = options.Value.WorkspaceConnection;

            NpgsqlConnectionStringBuilder connectionStringBuilder = workspaceConnection.ToConnectionStringBuilder();
            connectionStringBuilder.Database = database;
            connectionStringBuilder.Username = user.DbLogin;
            connectionStringBuilder.Password = user.DbPassword;

            NpgsqlConnection connection = new(connectionStringBuilder.ToString());

            if (workspaceConnection.CaCertificate is not null)
            {
                connection.ConfigureCaCertificateValidation(workspaceConnection.CaCertificate);
            }

            return connection;
        }
    }
}
