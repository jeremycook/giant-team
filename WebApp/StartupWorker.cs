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
            await Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    }
}
