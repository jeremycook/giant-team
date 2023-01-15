using GiantTeam.Cluster.Directory.Data;
using GiantTeam.Cluster.Directory.Helpers;
using GiantTeam.Cluster.Directory.Services;
using GiantTeam.Postgres;
using GiantTeam.Startup;
using GiantTeam.UserManagement.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Npgsql;

namespace GiantTeam.UserData.Services
{
    [Service]
    public class UserDirectoryDbContextFactory
    {
        private readonly IOptions<GiantTeamOptions> giantTeamOptions;
        private readonly SessionService sessionService;

        public UserDirectoryDbContextFactory(
            IOptions<GiantTeamOptions> giantTeamOptions,
            SessionService sessionService)
        {
            this.giantTeamOptions = giantTeamOptions;
            this.sessionService = sessionService;
        }

        public UserDirectoryDbContext NewDbContext()
        {
            var connectionStringBuilder = new NpgsqlConnectionStringBuilder(giantTeamOptions.Value.UserConnectionString)
            {
                Database = DirectoryHelpers.Database,
                SearchPath = DirectoryHelpers.Schema,
                Username = sessionService.User.DbLogin,
                Password = sessionService.User.DbPassword(),
            };

            var dbContextOptions =
                (DbContextOptions<UserDirectoryDbContext>)
                new DbContextOptionsBuilder<UserDirectoryDbContext>()
                .UseSnakeCaseNamingConvention()
                .UseNpgsql(connectionStringBuilder)
                .Options;

            return new UserDirectoryDbContext(dbContextOptions);
        }
    }
}
