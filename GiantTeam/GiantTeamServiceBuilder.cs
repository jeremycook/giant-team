using GiantTeam.Postgres;
using GiantTeam.RecordsManagement.Data;
using GiantTeam.Startup;
using GiantTeam.Startup.EnvVarFiles;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace GiantTeam;

public class GiantTeamServiceBuilder : IServiceBuilder
{
    public GiantTeamServiceBuilder(IServiceCollection services, ConfigurationManager configurationManager, IConfiguration configuration)
    {
        if (!string.IsNullOrEmpty(Environment.GetEnvironmentVariable("IN_CONTAINER")))
        {
            // Call this as early as possible!
            configurationManager.AddEnvVarFiles("/run/secrets");
        }

        services.Configure<GiantTeamOptions>(configuration);

        services.AddScopedFromAssembly(typeof(GiantTeamServiceBuilder).Assembly);

        services.AddDbContextPool<RecordsManagementDbContext>((services, options) =>
        {
            var giantTeamOptions = services.GetRequiredService<IOptions<GiantTeamOptions>>().Value;
            var connectionOptions = giantTeamOptions.MgmtConnection;

            options.UseNpgsql(connectionOptions);
        });
    }
}
