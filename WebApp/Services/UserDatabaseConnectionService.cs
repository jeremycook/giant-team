using Npgsql;

namespace WebApp.Services
{
    public class UserDatabaseConnectionService
    {
        private readonly string userConnectionString;
        private readonly SessionService sessionService;

        public UserDatabaseConnectionService(IConfiguration configuration, SessionService sessionService)
        {
            userConnectionString = configuration.GetConnectionString("User");
            this.sessionService = sessionService;
        }

        public NpgsqlConnection CreateConnection(string database)
        {
            SessionUser user = sessionService.User;

            NpgsqlConnectionStringBuilder connectionStringBuilder = new(userConnectionString)
            {
                Database = database,
                Username = UserHelper.DQL(user.DatabaseUsername, user.DatabaseSlot),
                Password = user.DatabasePassword,
            };

            return new(connectionStringBuilder.ToString());
        }
    }
}
