using Npgsql;

namespace WebApp.Services
{
    public class DatabaseConnectionService
    {
        private readonly string mainConnectionString;
        private readonly string userConnectionString;
        private readonly SessionService sessionService;

        public DatabaseConnectionService(IConfiguration configuration, SessionService sessionService)
        {
            mainConnectionString = configuration.GetConnectionString("Main");
            userConnectionString = configuration.GetConnectionString("User");
            this.sessionService = sessionService;
        }

        public NpgsqlConnection CreateAdminConnection(string database)
        {
            NpgsqlConnectionStringBuilder connectionStringBuilder = new(mainConnectionString)
            {
                Database = database,
            };

            return new(connectionStringBuilder.ToString());
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

            return new(connectionStringBuilder.ToString());
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

            return new(connectionStringBuilder.ToString());
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

            return new(connectionStringBuilder.ToString());
        }
    }
}
