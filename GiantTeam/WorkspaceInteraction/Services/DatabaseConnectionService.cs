using Dapper;
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

        public NpgsqlConnection CreateDesignConnection(string database)
        {
            SessionUser user = sessionService.User;
            var workspaceUserConnection = options.Value.WorkspaceConnection;

            NpgsqlConnectionStringBuilder connectionStringBuilder = new(workspaceUserConnection.ConnectionString)
            {
                Database = database,
                Username = DatabaseHelper.DesignUser(user.DatabaseUsername, user.DatabaseSlot),
                Password = user.DatabasePassword,
            };

            NpgsqlConnection connection = new(connectionStringBuilder.ToString());

            if (workspaceUserConnection.CaCertificate is not null)
            {
                connection.ConfigureCaCertificateValidation(workspaceUserConnection.CaCertificate);
            }

            connection.Open();
            connection.Execute($"SET ROLE {PgQuote.Identifier(DatabaseHelper.DesignUser(user.DatabaseUsername))};");
            return connection;
        }

        public NpgsqlConnection CreateManipulateConnection(string database)
        {
            SessionUser user = sessionService.User;
            var workspaceUserConnection = options.Value.WorkspaceConnection;

            NpgsqlConnectionStringBuilder connectionStringBuilder = new(workspaceUserConnection.ConnectionString)
            {
                Database = database,
                Username = DatabaseHelper.ManipulateUser(user.DatabaseUsername, user.DatabaseSlot),
                Password = user.DatabasePassword,
            };

            NpgsqlConnection connection = new(connectionStringBuilder.ToString());

            if (workspaceUserConnection.CaCertificate is not null)
            {
                connection.ConfigureCaCertificateValidation(workspaceUserConnection.CaCertificate);
            }

            connection.Open();
            connection.Execute($"SET ROLE {PgQuote.Identifier(DatabaseHelper.ManipulateUser(user.DatabaseUsername))};");
            return connection;
        }

        public NpgsqlConnection CreateQueryConnection(string database)
        {
            SessionUser user = sessionService.User;
            var workspaceUserConnection = options.Value.WorkspaceConnection;

            NpgsqlConnectionStringBuilder connectionStringBuilder = new(workspaceUserConnection.ConnectionString)
            {
                Database = database,
                Username = DatabaseHelper.QueryUser(user.DatabaseUsername, user.DatabaseSlot),
                Password = user.DatabasePassword,
            };

            NpgsqlConnection connection = new(connectionStringBuilder.ToString());

            if (workspaceUserConnection.CaCertificate is not null)
            {
                connection.ConfigureCaCertificateValidation(workspaceUserConnection.CaCertificate);
            }

            connection.Open();
            connection.Execute($"SET ROLE {PgQuote.Identifier(DatabaseHelper.QueryUser(user.DatabaseUsername))};");
            return connection;
        }
    }
}
