using GiantTeam.Cluster.Directory.Data;
using GiantTeam.Postgres;
using GiantTeam.Startup;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace GiantTeam;

public class GiantTeamServiceBuilder : IServiceBuilder
{
    public GiantTeamServiceBuilder(
        IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<GiantTeamOptions>(configuration);

        services.AddOrReplaceScopedFromAssembly(typeof(GiantTeamServiceBuilder).Assembly);

        services.AddDbContextPool<ManagerDirectoryDbContext>((services, options) =>
        {
            var giantTeamOptions = services.GetRequiredService<IOptions<GiantTeamOptions>>().Value;
            var connectionOptions = giantTeamOptions.DirectoryManagerConnection;

            options.UseSnakeCaseNamingConvention().UseNpgsql(connectionOptions);
        });
    }
}
