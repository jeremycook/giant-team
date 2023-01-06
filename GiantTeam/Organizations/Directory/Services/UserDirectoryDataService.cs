using GiantTeam.Organizations.Directory.Helpers;
using GiantTeam.Postgres;
using GiantTeam.UserManagement.Services;
using Microsoft.Extensions.Options;
using Npgsql;

namespace GiantTeam.Organizations.Directory.Services
{
    public class UserDirectoryDataService : PgDataService
    {
        public UserDirectoryDataService(
            ILogger<UserDirectoryDataService> logger,
            IOptions<GiantTeamOptions> options,
            SessionService sessionService)
            : base(logger, new NpgsqlConnectionStringBuilder(options.Value.UserConnectionString)
            {
                Database = DirectoryHelpers.Database,
                SearchPath = DirectoryHelpers.Schema,
                Username = sessionService.User.DbLogin,
                Password = sessionService.User.DbPassword
            }.ToString())
        {
        }
    }
}
