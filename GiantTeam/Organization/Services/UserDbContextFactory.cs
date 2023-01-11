using GiantTeam.ComponentModel;
using GiantTeam.Organization.Etc.Data;
using GiantTeam.Postgres;
using GiantTeam.Startup;
using GiantTeam.UserManagement.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Npgsql;

namespace GiantTeam.Organization.Services
{
    [Service]
    public class UserDbContextFactory
    {
        private readonly IOptions<GiantTeamOptions> giantTeamOptions;
        private readonly SessionService sessionService;

        public UserDbContextFactory(
            IOptions<GiantTeamOptions> giantTeamOptions,
            SessionService sessionService)
        {
            this.giantTeamOptions = giantTeamOptions;
            this.sessionService = sessionService;
        }

        public TDbContext NewDbContext<TDbContext>(string databaseName, string defaultSchema = "")
            where TDbContext : DbContext
        {
            var connectionStringBuilder = new NpgsqlConnectionStringBuilder(giantTeamOptions.Value.UserConnectionString)
            {
                Database = databaseName,
                SearchPath = defaultSchema,
                Username = sessionService.User.DbLogin,
                Password = sessionService.User.DbPassword
            };

            var dbContextOptions = new DbContextOptionsBuilder<EtcDbContext>()
                .UseSnakeCaseNamingConvention()
                .UseNpgsql(connectionStringBuilder)
                .Options;

            return (TDbContext)Activator.CreateInstance(
                type: typeof(TDbContext),
                args: new[] { (DbContextOptions<TDbContext>)dbContextOptions })!;
        }

        public TDbContext NewElevatedDbContext<TDbContext>(string databaseName, string defaultSchema = "")
            where TDbContext : DbContext
        {
            var connectionStringBuilder = new NpgsqlConnectionStringBuilder(giantTeamOptions.Value.UserConnectionString)
            {
                Database = databaseName,
                SearchPath = defaultSchema,
                Username = sessionService.User.DbElevatedLogin,
                Password = sessionService.User.DbPassword
            };

            var dbContextOptions = new DbContextOptionsBuilder<EtcDbContext>()
                .UseSnakeCaseNamingConvention()
                .UseNpgsql(connectionStringBuilder)
                .Options;

            return (TDbContext)Activator.CreateInstance(
                type: typeof(TDbContext),
                args: new[] { (DbContextOptions<TDbContext>)dbContextOptions })!;
        }
    }
}
