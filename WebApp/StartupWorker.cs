using GiantTeam;
using GiantTeam.DataProtection;
using GiantTeam.Postgres;
using GiantTeam.RecordsManagement.Data;
using Microsoft.Extensions.Options;

namespace WebApp
{
    public class StartupWorker : IHostedService
    {
        public StartupWorker(ILogger<StartupWorker> logger, IConfiguration configuration, IServiceProvider services)
        {
            Logger = logger;
            Configuration = configuration;
            Services = services;
        }

        public ILogger<StartupWorker> Logger { get; }
        public IConfiguration Configuration { get; }
        public IServiceProvider Services { get; }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            // TODO?
            //ConnectionOptions? migrationConnectionOptions = Configuration
            //    .GetSection("MigrationConnection")
            //    .Get<ConnectionOptions>();

            //if (migrationConnectionOptions is not null)
            //{
            //    DataProtectionOptions dataProtectionOptions = Services.GetRequiredService<IOptions<DataProtectionOptions>>().Value;
            //    GiantTeamOptions giantTeamOptions = Services.GetRequiredService<IOptions<GiantTeamOptions>>().Value;

            //    try
            //    {
            //        await Services.MigrateDbContextAsync<DataProtectionDbContext>(migrationConnectionOptions, dataProtectionOptions.DataProtectionConnection);
            //        await Services.MigrateDbContextAsync<RecordsManagementDbContext>(migrationConnectionOptions, giantTeamOptions.MgmtConnection);
            //    }
            //    catch (Exception ex)
            //    {
            //        Logger.LogError(ex, "Suppressed migration exception {Exception}: {ExceptionMessage}", ex.GetBaseException(), ex.GetBaseException().Message);
            //    }
            //}

            await Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    }
}
