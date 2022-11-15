using GiantTeam.Postgres;
using GiantTeam.Services;
using Microsoft.Extensions.Options;
using Npgsql;

namespace GiantTeam.WorkspaceInteraction.Services
{
    public class WorkspaceConnectionService
    {
        private readonly IOptions<GiantTeamOptions> options;
        private readonly SessionService sessionService;

        public WorkspaceConnectionService(IOptions<GiantTeamOptions> options, SessionService sessionService)
        {
            this.options = options;
            this.sessionService = sessionService;
        }

        public NpgsqlConnection OpenMaintenanceConnection(string workspaceId)
        {
            string? maintenanceDatabase =
                options.Value.WorkspaceConnection.MaintenanceDatabase ??
                throw new InvalidOperationException("Maintenance database not set.");

            NpgsqlConnection connection = CreateConnection(maintenanceDatabase);
            connection.Open();
            connection.SetRole(workspaceId);
            return connection;
        }

        public async Task<NpgsqlConnection> OpenMaintenanceConnectionAsync(string workspaceId)
        {
            string? maintenanceDatabase =
                options.Value.WorkspaceConnection.MaintenanceDatabase ??
                throw new InvalidOperationException("Maintenance database not set.");

            NpgsqlConnection connection = CreateConnection(maintenanceDatabase);
            await connection.OpenAsync();
            await connection.SetRoleAsync(workspaceId);
            return connection;
        }

        public NpgsqlConnection OpenAdminConnection(string workspaceId)
        {
            NpgsqlConnection connection = CreateConnection(workspaceId);
            connection.Open();
            connection.SetRole(workspaceId);
            return connection;
        }

        public async Task<NpgsqlConnection> OpenAdminConnectionAsync(string workspaceId)
        {
            NpgsqlConnection connection = CreateConnection(workspaceId);
            await connection.OpenAsync();
            await connection.SetRoleAsync(workspaceId);
            return connection;
        }

        public NpgsqlConnection OpenUserConnection(string workspaceId)
        {
            SessionUser user = sessionService.User;

            NpgsqlConnection connection = CreateConnection(workspaceId);
            connection.Open();
            connection.SetRole(user.DbRole);
            return connection;
        }

        public async Task<NpgsqlConnection> OpenUserConnectionAsync(string workspaceId)
        {
            SessionUser user = sessionService.User;

            NpgsqlConnection connection = CreateConnection(workspaceId);
            await connection.OpenAsync();
            await connection.SetRoleAsync(user.DbRole);
            return connection;
        }

        public NpgsqlConnection CreateConnection(string workspaceId)
        {
            SessionUser user = sessionService.User;
            var workspaceConnection = options.Value.WorkspaceConnection;

            NpgsqlConnectionStringBuilder connectionStringBuilder = workspaceConnection.ToConnectionStringBuilder();
            connectionStringBuilder.Database = workspaceId;
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
