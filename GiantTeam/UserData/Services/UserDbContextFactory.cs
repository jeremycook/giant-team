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
    public class UserDbContextFactory
    {
        private readonly IOptions<GiantTeamOptions> giantTeamOptions;
        private readonly GetDatabaseNameService getDatabaseNameService;
        private readonly SessionService sessionService;

        public UserDbContextFactory(
            IOptions<GiantTeamOptions> giantTeamOptions,
            GetDatabaseNameService getDatabaseNameService,
            SessionService sessionService)
        {
            this.giantTeamOptions = giantTeamOptions;
            this.getDatabaseNameService = getDatabaseNameService;
            this.sessionService = sessionService;
        }

        public TDbContext NewDbContext<TDbContext>(string organizationId, string defaultSchema = "")
            where TDbContext : DbContext
        {
            var databaseName = getDatabaseNameService.GetDatabaseName(organizationId);

            var connectionStringBuilder = new NpgsqlConnectionStringBuilder(giantTeamOptions.Value.UserConnectionString)
            {
                Database = databaseName,
                SearchPath = defaultSchema,
                Username = sessionService.User.DbLogin,
                Password = sessionService.User.DbPassword(),
            };

            var dbContextOptions =
                (DbContextOptions<TDbContext>)
                new DbContextOptionsBuilder<TDbContext>()
                .UseSnakeCaseNamingConvention()
                .UseNpgsql(connectionStringBuilder)
                .Options;

            return (TDbContext)Activator.CreateInstance(
                type: typeof(TDbContext),
                args: new[] { dbContextOptions })!;
        }

        public TDbContext NewElevatedDbContext<TDbContext>(string organizationId, string defaultSchema = "")
            where TDbContext : DbContext
        {
            var databaseName = getDatabaseNameService.GetDatabaseName(organizationId);

            var connectionStringBuilder = new NpgsqlConnectionStringBuilder(giantTeamOptions.Value.UserConnectionString)
            {
                Database = databaseName,
                SearchPath = defaultSchema,
                Username = sessionService.User.DbElevatedLogin,
                Password = sessionService.User.DbPassword(),
            };

            var dbContextOptions = new DbContextOptionsBuilder<TDbContext>()
                .UseSnakeCaseNamingConvention()
                .UseNpgsql(connectionStringBuilder)
                .Options;

            return (TDbContext)Activator.CreateInstance(
                type: typeof(TDbContext),
                args: new[] { (DbContextOptions<TDbContext>)dbContextOptions })!;
        }
    }
}
