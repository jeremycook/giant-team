using GiantTeam.Postgres;
using GiantTeam.RecordsManagement.Data;
using GiantTeam.Startup;
using GiantTeam.Startup.DatabaseConfiguration;
using GiantTeam.Startup.EnvVarFiles;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace GiantTeam;

public class GiantTeamServiceBuilder : IServiceBuilder
{
    public GiantTeamServiceBuilder(
        IHostEnvironment environment,
        IServiceCollection services,
        ConfigurationManager configurationManager)
    {
        if (!string.IsNullOrEmpty(Environment.GetEnvironmentVariable("IN_CONTAINER")))
        {
            // Call this as early as possible!
            configurationManager.AddEnvVarFiles("/run/secrets");
        }

        var file = configurationManager.GetValue<string>("APPSETTINGS_CONNECTION_OPTIONS_FILE");
        if (!string.IsNullOrEmpty(file))
        {
            if (!File.Exists(file))
            {
                throw new FileNotFoundException($"The APPSETTINGS_CONNECTION_OPTIONS_FILE was not found at: {file}.");
            }

            var connectionConfiguration = new ConfigurationBuilder()
                .SetBasePath(environment.ContentRootPath)
                .AddJsonFile(file)
                .Build();

            var connectionOptions =
                connectionConfiguration.Get<ConnectionOptions>() ??
                throw new NullReferenceException();

            configurationManager.AddDatabase(connectionOptions);
        }

        services.Configure<GiantTeamOptions>(configurationManager);

        services.AddScopedFromAssembly(typeof(GiantTeamServiceBuilder).Assembly);

        services.AddDbContextPool<RecordsManagementDbContext>((services, options) =>
        {
            var giantTeamOptions = services.GetRequiredService<IOptions<GiantTeamOptions>>().Value;
            var connectionOptions = giantTeamOptions.MgmtConnection;

            options.UseNpgsql(connectionOptions);
        });
    }
}
