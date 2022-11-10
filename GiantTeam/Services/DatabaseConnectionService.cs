using GiantTeam.Postgres;
using Microsoft.Extensions.Options;
using Npgsql;

namespace GiantTeam.Services
{
    public class DatabaseConnectionService
    {
        private string userConnectionString => options.Value.WorkspaceConnection.ConnectionString;
        private readonly IOptions<GiantTeamOptions> options;
        private readonly SessionService sessionService;

        public DatabaseConnectionService(IOptions<GiantTeamOptions> options, SessionService sessionService)
        {
            this.options = options;
            this.sessionService = sessionService;
        }

        public NpgsqlConnection CreateDesignConnection(string database)
        {
            SessionUser user = sessionService.User;

            NpgsqlConnectionStringBuilder connectionStringBuilder = new(userConnectionString)
            {
                Database = database,
                Username = DatabaseHelper.DesignUser(user.DatabaseUsername, user.DatabaseSlot),
                Password = user.DatabasePassword,
            };

            NpgsqlConnection connection = new(connectionStringBuilder.ToString());

            if (options.Value.WorkspaceConnection.CaCertificate is string connectionCaCertificate)
            {
                connection.ConfigureCaCertificateValidation(connectionCaCertificate);
            }

            return connection;
        }

        public NpgsqlConnection CreateManipulateConnection(string database)
        {
            SessionUser user = sessionService.User;

            NpgsqlConnectionStringBuilder connectionStringBuilder = new(userConnectionString)
            {
                Database = database,
                Username = DatabaseHelper.ManipulateUser(user.DatabaseUsername, user.DatabaseSlot),
                Password = user.DatabasePassword,
            };

            NpgsqlConnection connection = new(connectionStringBuilder.ToString());

            if (options.Value.WorkspaceConnection.CaCertificate is string connectionCaCertificate)
            {
                connection.ConfigureCaCertificateValidation(connectionCaCertificate);
            }

            return connection;
        }

        public NpgsqlConnection CreateQueryConnection(string database)
        {
            SessionUser user = sessionService.User;

            NpgsqlConnectionStringBuilder connectionStringBuilder = new(userConnectionString)
            {
                Database = database,
                Username = DatabaseHelper.QueryUser(user.DatabaseUsername, user.DatabaseSlot),
                Password = user.DatabasePassword,
            };

            NpgsqlConnection connection = new(connectionStringBuilder.ToString());

            if (options.Value.WorkspaceConnection.CaCertificate is string connectionCaCertificate)
            {
                connection.ConfigureCaCertificateValidation(connectionCaCertificate);
            }

            return connection;
        }
    }
}
