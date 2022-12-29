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

        public SessionUser User => sessionService.User;

        public UserConnectionService(IOptions<GiantTeamOptions> options, SessionService sessionService)
        {
            this.options = options;
            this.sessionService = sessionService;
        }

        public NpgsqlConnection OpenInfoConnection(string setRole)
        {
            string? maintenanceDatabase =
                options.Value.InfoDatabaseName ??
                throw new InvalidOperationException("InfoDatabaseName not set.");

            NpgsqlConnection connection = CreateConnection(maintenanceDatabase);
            connection.Open();
            connection.SetRole(setRole);
            return connection;
        }

        public NpgsqlConnection OpenInfoConnection()
        {
            return OpenInfoConnection(sessionService.User.DbRole);
        }

        public async Task<NpgsqlConnection> OpenInfoConnectionAsync(string setRole)
        {
            string? maintenanceDatabase =
                options.Value.InfoDatabaseName ??
                throw new InvalidOperationException("InfoDatabaseName not set.");

            NpgsqlConnection connection = CreateConnection(maintenanceDatabase);
            await connection.OpenAsync();
            await connection.SetRoleAsync(setRole);
            return connection;
        }

        public async Task<NpgsqlConnection> OpenInfoConnectionAsync()
        {
            return await OpenInfoConnectionAsync(sessionService.User.DbRole);
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

        public NpgsqlConnection OpenConnection(string databaseName, bool setRole = true)
        {
            if (setRole)
            {
                SessionUser user = sessionService.User;
                return OpenConnection(databaseName, user.DbRole);
            }
            else
            {
                NpgsqlConnection connection = CreateConnection(databaseName);
                connection.Open();
                return connection;
            }
        }

        /// <summary>
        /// Open a connection to the <paramref name="databaseName"/>
        /// with role set to <see cref="SessionUser.DbRole"/>.
        /// </summary>
        /// <param name="databaseName"></param>
        /// <returns></returns>
        public async Task<NpgsqlConnection> OpenConnectionAsync(string databaseName, bool setRole = true)
        {
            if (setRole)
            {
                SessionUser user = sessionService.User;
                return await OpenConnectionAsync(databaseName, user.DbRole);
            }
            else
            {
                NpgsqlConnection connection = CreateConnection(databaseName);
                await connection.OpenAsync();
                return connection;
            }
        }

        /// <summary>
        /// Creates a connection with <see cref="SessionUser.DbLogin"/> username.
        /// </summary>
        /// <param name="databaseName"></param>
        /// <returns></returns>
        public NpgsqlConnection CreateConnection(string databaseName)
        {
            SessionUser user = sessionService.User;
            var workspaceConnection = options.Value.UserConnection;

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
