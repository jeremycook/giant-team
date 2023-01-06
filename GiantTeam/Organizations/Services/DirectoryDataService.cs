using GiantTeam.Organizations.Directory.Helpers;
using GiantTeam.Postgres;
using GiantTeam.UserManagement.Services;
using Microsoft.Extensions.Options;
using Npgsql;

namespace GiantTeam.Organizations.Services
{
    [Obsolete("TODO: Rename to UserDirectoryDataService.")]
    public class DirectoryDataService : PgDataService
    {
        public DirectoryDataService(
            ILogger<DirectoryDataService> logger,
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
