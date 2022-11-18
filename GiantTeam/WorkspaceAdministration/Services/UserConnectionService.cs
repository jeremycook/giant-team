using GiantTeam.Postgres;
using GiantTeam.UserManagement.Services;
using Microsoft.Extensions.Options;
using Npgsql;

namespace GiantTeam.WorkspaceAdministration.Services
{
    public class UserConnectionService
    {
        private readonly IOptions<GiantTeamOptions> options;
        private readonly SessionService sessionService;

        public UserConnectionService(IOptions<GiantTeamOptions> options, SessionService sessionService)
        {
            this.options = options;
            this.sessionService = sessionService;
        }

        public NpgsqlConnection OpenMaintenanceConnection(string setRole)
        {
            string? maintenanceDatabase =
                options.Value.WorkspaceConnection.MaintenanceDatabase ??
                throw new InvalidOperationException("Maintenance database not set.");

            NpgsqlConnection connection = CreateConnection(maintenanceDatabase);
            connection.Open();
            connection.SetRole(setRole);
            return connection;
        }

        public NpgsqlConnection OpenMaintenanceConnection()
        {
            return OpenMaintenanceConnection(sessionService.User.DbRole);
        }

        public async Task<NpgsqlConnection> OpenMaintenanceConnectionAsync(string setRole)
        {
            string? maintenanceDatabase =
                options.Value.WorkspaceConnection.MaintenanceDatabase ??
                throw new InvalidOperationException("Maintenance database not set.");

            NpgsqlConnection connection = CreateConnection(maintenanceDatabase);
            await connection.OpenAsync();
            await connection.SetRoleAsync(setRole);
            return connection;
        }

        public async Task<NpgsqlConnection> OpenMaintenanceConnectionAsync()
        {
            return await OpenMaintenanceConnectionAsync(sessionService.User.DbRole);
        }

        public NpgsqlConnection OpenConnection(string databaseName, string setRole)
        {
            NpgsqlConnection connection = CreateConnection(databaseName);
            connection.Open();
            connection.SetRole(setRole);
            return connection;
        }

        public async Task<NpgsqlConnection> OpenConnectionAsync(string databaseName, string setRole)
        {
            NpgsqlConnection connection = CreateConnection(databaseName);
            await connection.OpenAsync();
            await connection.SetRoleAsync(setRole);
            return connection;
        }

        public NpgsqlConnection OpenConnection(string databaseName)
        {
            SessionUser user = sessionService.User;

            NpgsqlConnection connection = CreateConnection(databaseName);
            connection.Open();
            connection.SetRole(user.DbRole);
            return connection;
        }

        /// <summary>
        /// Open a connection to the <paramref name="databaseName"/>
        /// with role set to <see cref="SessionUser.DbRole"/>.
        /// </summary>
        /// <param name="databaseName"></param>
        /// <returns></returns>
        public async Task<NpgsqlConnection> OpenConnectionAsync(string databaseName)
        {
            SessionUser user = sessionService.User;

            NpgsqlConnection connection = CreateConnection(databaseName);
            await connection.OpenAsync();
            await connection.SetRoleAsync(user.DbRole);
            return connection;
        }

        /// <summary>
        /// Creates a connection with <see cref="SessionUser.DbLogin"/> username.
        /// </summary>
        /// <param name="databaseName"></param>
        /// <returns></returns>
        public NpgsqlConnection CreateConnection(string databaseName)
        {
            SessionUser user = sessionService.User;
            var workspaceConnection = options.Value.WorkspaceConnection;

            NpgsqlConnectionStringBuilder connectionStringBuilder = workspaceConnection.ToConnectionStringBuilder();
            connectionStringBuilder.Database = databaseName;
            connectionStringBuilder.Username = user.DbLogin;
            connectionStringBuilder.Password = user.DbPassword;

            NpgsqlConnection connection = new(connectionStringBuilder.ToString());

            if (!string.IsNullOrEmpty(workspaceConnection.CaCertificate))
            {
                connection.ConfigureCaCertificateValidation(workspaceConnection.CaCertificate);
            }

            return connection;
        }
    }
}
