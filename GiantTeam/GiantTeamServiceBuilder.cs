using GiantTeam.Postgres;
using GiantTeam.RecordsManagement.Data;
using GiantTeam.Startup;
using GiantTeam.WorkspaceAdministration.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace GiantTeam;

public class GiantTeamServiceBuilder : IServiceBuilder
{
    public GiantTeamServiceBuilder(IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<GiantTeamOptions>(configuration.GetRequiredSection("GiantTeam"));

        services.AddScopedFromAssembly(typeof(GiantTeamServiceBuilder).Assembly);

        services.AddPooledDbContextFactory<RecordsManagementDbContext>((services, options) =>
        {
            var giantTeamOptions = services.GetRequiredService<IOptions<GiantTeamOptions>>().Value;
            var connectionOptions = giantTeamOptions.RecordsManagementConnection;

            options.UseNpgsql(connectionOptions);
        });

        services.AddDbContextPool<WorkspaceAdministrationDbContext>((services, options) =>
        {
            var giantTeamOptions = services.GetRequiredService<IOptions<GiantTeamOptions>>().Value;
            var connectionOptions = giantTeamOptions.WorkspaceAdministrationConnection;

            options.UseNpgsql(connectionOptions);
        });
    }
}
